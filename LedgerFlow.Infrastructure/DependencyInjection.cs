using LedgerFlow;
using LedgerFlow.Infrastructure;
using LedgerFlow.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Claims;
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
    }

    public static void AddLedgerFlowDbContextCheck(this IServiceCollection services)
    {
        services.AddHealthChecks()
                .AddCheck<DbContextHealthCheck<LedgerFlowDbContext>>(nameof(LedgerFlowDbContext));
    }
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<ITransactionRepository, TransactionRepository>();
        services.AddTransient<ILedgerSummaryRepository, LedgerSummaryRepository>();
    }
    private static void AddDbContext(this IHostApplicationBuilder builder)
    {
        var LedgerFlowConnectionStringKey = "LedgerFlow";
        Console.WriteLine($"Trying to get a database connectionString '{LedgerFlowConnectionStringKey}' from Configuration.");
        var LedgerFlowConnectionString = builder.Configuration.GetConnectionString(LedgerFlowConnectionStringKey);
        if (LedgerFlowConnectionString == null)
        {
            Console.WriteLine("LedgerFlow ConnectionString NOT found, using InMemoryDatabase for LedgerFlowDbContext.");
            builder.Services.AddDbContext<LedgerFlowDbContext>(options => options.UseInMemoryDatabase(nameof(LedgerFlowDbContext)));
        }
        else
        {
            Console.WriteLine($"Using LedgerFlow ConnectionString for LedgerFlowDbContext.");
            builder.Services.AddDbContext<LedgerFlowDbContext>(options => options.UseSqlServer(LedgerFlowConnectionString));
        }
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
}
