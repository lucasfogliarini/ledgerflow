
using Microsoft.Extensions.Logging;

namespace LedgerFlow.LedgerSummaries.Application;

public class TransactionCreatedHandler(ITransactionRepository transactionRepository, ILogger<TransactionCreatedHandler> logger)
{

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

        logger.LogInformation("Transação de criada com sucesso: {TransactionId}", transaction.Id);
    }
}