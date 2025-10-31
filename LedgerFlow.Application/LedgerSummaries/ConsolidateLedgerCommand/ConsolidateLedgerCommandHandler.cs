using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace LedgerFlow.Application.LedgerSummaries;

public class ConsolidateLedgerCommandHandler(ILedgerSummaryRepository ledgerSummaryRepository, ITransactionRepository transactionRepository, ILogger<ConsolidateLedgerCommandHandler> logger) : ICommandHandler<ConsolidateLedgerCommand>
{
    public async Task<Result> HandleAsync(ConsolidateLedgerCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null)
            return Result.Failure("O comando não pode ser nulo.");

        var transactions = await transactionRepository.GetTransactionsAsync(DateOnly.FromDateTime(command.ReferenceDate), cancellationToken);

        if (!transactions.Any())
            return Result.Failure("Nenhuma transação encontrada para consolidação.");

        var ledgerSummary = new LedgerSummary(command.ReferenceDate);
        ledgerSummary.AddTransactions(transactions);

        ledgerSummaryRepository.Add(ledgerSummary);

        await ledgerSummaryRepository.CommitScope.CommitAsync(cancellationToken);

        logger.LogInformation("Consolidação de {Date} concluída com sucesso.", command.ReferenceDate);
        return Result.Success();
    }
}

public record ConsolidateLedgerCommand(DateTime ReferenceDate) : ICommand;
