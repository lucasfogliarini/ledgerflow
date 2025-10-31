using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace LedgerFlow.Application.Transactions;

public class ConsolidateLedgerCommandHandler(ILogger<ConsolidateLedgerCommandHandler> logger) : ICommandHandler<ConsolidateLedgerCommand>
{
    public async Task<Result> HandleAsync(ConsolidateLedgerCommand command, CancellationToken cancellationToken = default)
    {
        //if (command is null)
        //    return Result.Failure("O comando não pode ser nulo.");

        //var transactions = await _transactions.GetByDateAsync(command.ReferenceDate, cancellationToken);

        //if (!transactions.Any())
        //    return Result.Failure("Nenhuma transação encontrada para consolidação.");

        //var ledgerSummary = new LedgerSummary(command.ReferenceDate);
        //ledgerSummary.AddTransaction(transactions);

        ////await _summaries.UpsertAsync(summary, cancellationToken);

        //if (result.IsFailure)
        //{
        //    logger.LogWarning("Falha na consolidação do dia {Date}: {Error}", command.ReferenceDate, result.Error);
        //    return result;
        //}

        //logger.LogInformation("Consolidação de {Date} concluída com sucesso.", command.ReferenceDate);
        return Result.Success();
    }
}

public record ConsolidateLedgerCommand(DateTime ReferenceDate) : ICommand;
