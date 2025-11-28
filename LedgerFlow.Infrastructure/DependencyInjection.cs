using LedgerFlow;
using LedgerFlow.Infrastructure;
using LedgerFlow.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddDbContext();        
        builder.Services.AddRepositories();
        builder.AddOpenTelemetryExporter();
        builder.AddRateLimiter();
        builder.AddLivenessHealthCheck();
    }
    public static void Migrate(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LedgerFlowDbContext>();

        db.Database.Migrate();
    }
    public static void MapHealthChecks(this WebApplication app)
    {
        var serviceInfo = ServiceInfo.Get();
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("live")
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    serviceInfo.Name,
                    serviceInfo.Version,
                    app.Environment.EnvironmentName,
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                    })
                });

                await context.Response.WriteAsync(result);
            }
        });
    }
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<ITransactionRepository, TransactionRepository>();
        services.AddTransient<ILedgerSummaryRepository, LedgerSummaryRepository>();
    }
    private static void AddDbContext(this IHostApplicationBuilder builder, string ledgerFlowConnectionStringKey = "LedgerFlow")
    {
        var ledgerFlowConnectionString = builder.Configuration.GetConnectionString(ledgerFlowConnectionStringKey);

        void BuilderOptions(DbContextOptionsBuilder options)
        {
            if (ledgerFlowConnectionString is not null)
                options.UseSqlServer(ledgerFlowConnectionString);
            else
                options.UseInMemoryDatabase(nameof(LedgerFlowDbContext));

            // Use the following options only during development or troubleshooting
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }

        builder.Services.AddDbContext<LedgerFlowDbContext>(BuilderOptions);
        builder.Services.AddHealthChecks()
            .AddCheck<DbContextHealthCheck<LedgerFlowDbContext>>(nameof(LedgerFlowDbContext));
    }
    private static void AddOpenTelemetryExporter(this IHostApplicationBuilder builder)
    {
        var serviceInfo = ServiceInfo.Get();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(rb => rb.AddService(serviceInfo.Name, null, serviceInfo.Version))
            .WithTracing(tracerBuilder =>
            {
                tracerBuilder
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter();
                //.AddConsoleExporter();
            })
            .WithMetrics(meterBuilder =>
            {
                meterBuilder
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter();
                //.AddConsoleExporter();
            })
            .WithLogging(loggingBuilder =>
            {
                loggingBuilder
                    .AddOtlpExporter();
                //.AddConsoleExporter();
            });

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
        });
    }
    private static void AddRateLimiter(this IHostApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("per-user", context =>
            {
                var key = context.User.FindFirstValue("sid") ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: key,
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 100,
                        TokensPerPeriod = 50,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(30)
                    });
            });
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Limite atingido, tente novamente em breve.", token);
            };
        });
    }
    private static void AddLivenessHealthCheck(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    }
}
