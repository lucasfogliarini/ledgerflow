using LedgerFlow;
using LedgerFlow.LedgerSummaries.Infrastructure;
using LedgerFlow.LedgerSummaries.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Kafka;
using Wolverine.SqlServer;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddDbContext<LedgerSummariesDbContext>("LedgerSummariesDatabase");        
        builder.Services.AddRepositories();
        builder.AddOpenTelemetryExporter();
        builder.AddRateLimiter();
        builder.AddLivenessHealthCheck();
    }
    public static void UseWolverineFx(this IHostApplicationBuilder builder, Assembly assembly)
    {
        builder.UseWolverine(opts =>
        {
            var wolverineConnectionString = builder.Configuration.GetConnectionString("WolverineDatabase");
            opts.PersistMessagesWithSqlServer(wolverineConnectionString);
            opts.Discovery.IncludeAssembly(assembly);
            opts.UseEntityFrameworkCoreTransactions();
            opts.PublishDomainEventsFromEntityFrameworkCore<AggregateRoot>(e => e.DomainEvents);
            opts.Policies.AutoApplyTransactions();

            var connString = builder.Configuration.GetConnectionString("kafka");
            opts.UseKafka(connString)
                .ConfigureConsumers(consumer =>
                {
                    consumer.GroupId = "ledgersummaries-group";
                });

            opts.ListenToKafkaTopic("transaction-created")
                .ReceiveRawJson<TransactionCreated>();
                //.UseDurableInbox();
        });
    }

    public static void Migrate(this WebApplication app)
    {
        app.Migrate<LedgerSummariesDbContext>();
    }
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<ITransactionRepository, TransactionRepository>();
        services.AddTransient<ILedgerSummaryRepository, LedgerSummaryRepository>();
    }
}
