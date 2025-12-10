using LedgerFlow.Infrastructure;
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
using Wolverine.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
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
    public static void AddDbContext<TDbContext>(this IHostApplicationBuilder builder, string connectionStringKey) where TDbContext: DbContext
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionStringKey);

        void BuilderOptions(DbContextOptionsBuilder options)
        {
            if (connectionString is not null)
                options.UseSqlServer(connectionString);
            else
                options.UseInMemoryDatabase(nameof(TDbContext));

            // Use the following options only during development or troubleshooting
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }

        builder.Services.AddDbContextWithWolverineIntegration<TDbContext>(BuilderOptions);
        //builder.Services.AddDbContext<TDbContext>(BuilderOptions);
        builder.Services.AddHealthChecks()
            .AddCheck<DbContextHealthCheck<TDbContext>>(nameof(TDbContext));
    }
    public static void AddOpenTelemetryExporter(this IHostApplicationBuilder builder)
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
            })
            .WithMetrics(meterBuilder =>
            {
                meterBuilder
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter();
            })
            .WithLogging(loggingBuilder =>
            {
                loggingBuilder
                    .AddOtlpExporter();
            });

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
        });
    }
    public static void AddRateLimiter(this IHostApplicationBuilder builder)
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
    public static void AddLivenessHealthCheck(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    }
    internal static void Migrate<TDbContext>(this WebApplication app) where TDbContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        if (db.Database.IsRelational())
            db.Database.Migrate();
    }
}
