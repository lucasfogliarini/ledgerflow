using LedgerFlow;
using LedgerFlow.Transactions.Infrastructure;
using LedgerFlow.Transactions.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Kafka;
using Wolverine.SqlServer;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddDbContext<TransactionsDbContext>("TransactionsDatabase");        
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
            opts.PublishDomainEventsFromEntityFrameworkCore<AggregateRoot>(e=>e.DomainEvents);
            opts.Policies.AutoApplyTransactions();

            var connString = builder.Configuration.GetConnectionString("kafka");
            opts.UseKafka(connString);

            opts.PublishMessage<TransactionCreated>()
                .ToKafkaTopic("transaction-created")
                .UseDurableOutbox();
        });
    }
    public static void Migrate(this WebApplication app)
    {
        app.Migrate<TransactionsDbContext>();
    }
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<ITransactionRepository, TransactionRepository>();
    }
}
