using LedgerFlow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Kafka;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddTransactionsModule(this IHostApplicationBuilder builder)
    {
        builder.UseWolverineFx();
    }

    private static void UseWolverineFx(this IHostApplicationBuilder builder)
    {
        builder.UseWolverine(opts =>
        {
            var connString = builder.Configuration.GetConnectionString("kafka");
            opts.UseKafka(connString);

            opts.PublishMessage<TransactionCreated>()
                .ToKafkaTopic("transaction-created");
        });
    }
}
