using LedgerFlow;
using LedgerFlow.Infrastructure;
using LedgerFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IHostApplicationBuilder builder)
        {
            builder.AddDbContext();
            builder.Services.AddRepositories();
            builder.AddOpenTelemetryExporter();
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
            var assemblyName = Assembly.GetEntryAssembly()?.GetName();
            var serviceName = assemblyName?.Name ?? "Unknown Service Name";
            var serviceVersion = assemblyName?.Version?.ToString() ?? "Unknown Version";

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(rb => rb.AddService(serviceName, null, serviceVersion))
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
                        .AddOtlpExporter()
                        .AddConsoleExporter();
                });

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
            });
        }
    }
}
