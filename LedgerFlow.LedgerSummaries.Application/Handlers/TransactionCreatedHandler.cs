
using Microsoft.Extensions.Logging;
using Wolverine.ErrorHandling;
using Wolverine.Runtime.Handlers;

namespace LedgerFlow.LedgerSummaries.Application;

public class TransactionCreatedHandler(ITransactionRepository transactionRepository, ILogger<TransactionCreatedHandler> logger)
{
    public static void Configure(HandlerChain chain)
    {
        chain.OnAnyException()
            .RetryTimes(3)
            .Then.Discard().And(async (r, context, _) =>
            {
                var dql = $"{context.Envelope.TopicName}.dlq";
                r.Logger.LogInformation($"Sending to DLQ: {dql}.");
                await context.BroadcastToTopicAsync(dql, context.Envelope.Message);
            });
    }

    public async Task HandleAsync(TransactionCreated transactionCreated, CancellationToken cancellationToken = default)
    {

    }
}