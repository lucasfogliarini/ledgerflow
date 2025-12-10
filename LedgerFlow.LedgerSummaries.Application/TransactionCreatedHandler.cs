
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
        var transaction = Transaction.CreateTransaction(
                                transactionCreated.Type,
                                transactionCreated.Value,
                                transactionCreated.CreatedAt);

        logger.LogInformation("Criando transação: {Id} | {Value:C} | {Description}",
            transaction.Id,
            transaction.Value,
            transaction.Description);

        transactionRepository.Add(transaction);
        await transactionRepository.CommitScope.CommitAsync(cancellationToken);

        logger.LogInformation("Transação de criada com sucesso: {TransactionId}", transaction.Id);
    }
}