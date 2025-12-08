using LedgerFlow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Kafka;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddLedgerSummariesModule(this IHostApplicationBuilder builder)
    {
        builder.UseWolverineFx();
    }

    private static void UseWolverineFx(this IHostApplicationBuilder builder)
    {
        builder.UseWolverine(opts =>
        {
            var connString = builder.Configuration.GetConnectionString("kafka");
            opts.UseKafka(connString)
                .ConfigureConsumers(consumer =>
                {
                    consumer.GroupId = "kafka-group";
                });

            opts.ListenToKafkaTopic("transaction-created")
                .ReceiveRawJson<TransactionCreated>();
        });
    }
}