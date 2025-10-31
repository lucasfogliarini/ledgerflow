using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace LedgerFlow.Application.Transactions;

public class CreateDebitCommandHandler(ITransactionRepository transactionRepository, ILogger<CreateDebitCommandHandler> logger) : ICommandHandler<CreateDebitCommand, Transaction>
{
    public async Task<Result<Transaction>> HandleAsync(CreateDebitCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null)
            return Result.Failure<Transaction>("O comando não pode ser nulo.");

        var result = Transaction.CreateDebit(command.Value, command.Description);

        if (result.IsFailure)
        {
            logger.LogWarning("Falha ao criar transação de débito: {Error}", result.Error);
            return Result.Failure<Transaction>(result.Error);
        }

        var transaction = result.Value;

        logger.LogInformation("Criando transação de crédito: {Id} | {Value:C} | {Description}",
            transaction.Id,
            transaction.Value,
            transaction.Description);

        transactionRepository.Add(transaction);
        await transactionRepository.CommitScope.CommitAsync(cancellationToken);

        logger.LogInformation("Transação de crédito criada com sucesso: {TransactionId}", transaction.Id);

        return Result.Success(transaction);
    }
}

public record CreateDebitCommand(decimal Value, string Description) : ICommand<Transaction>;
