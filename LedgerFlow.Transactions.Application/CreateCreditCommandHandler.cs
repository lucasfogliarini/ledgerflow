using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace LedgerFlow.Transactions.Application;

public class CreateCreditCommandHandler(ITransactionRepository transactionRepository,  ILogger<CreateCreditCommandHandler> logger)
{
    public async Task<Result<Transaction>> HandleAsync(CreateCreditCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null)
            return Result.Failure<Transaction>("O comando não pode ser nulo.");

        var result = Transaction.CreateCredit(command.Value, command.Description);

        if (result.IsFailure)
        {
            logger.LogWarning("Falha ao criar transação de crédito: {Error}", result.Error);
            return Result.Failure<Transaction>(result.Error);
        }

        var transaction = result.Value;

        logger.LogInformation("Criando transação de crédito: {Id} | {Value:C} | {Description}",
            transaction.Id,
            transaction.Value,
            transaction.Description);

        transactionRepository.Add(transaction);
        logger.LogInformation("Transação de crédito criada com sucesso: {TransactionId}", transaction.Id);

        return Result.Success(transaction);
    }
}

public record CreateCreditCommand(decimal Value, string Description);
