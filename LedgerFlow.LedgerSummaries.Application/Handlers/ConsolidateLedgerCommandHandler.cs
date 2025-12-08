using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace LedgerFlow.LedgerSummaries.Application;

public class ConsolidateLedgerCommandHandler(ILedgerSummaryRepository ledgerSummaryRepository, ITransactionRepository transactionRepository, ILogger<ConsolidateLedgerCommandHandler> logger)
{
    public async Task<Result<LedgerSummaryResponse>> HandleAsync(ConsolidateLedgerCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null)
            return Result.Failure<LedgerSummaryResponse>("O comando não pode ser nulo.");

        var transactions = await transactionRepository.GetTransactionsAsync(DateOnly.FromDateTime(command.ReferenceDate), cancellationToken);

        if (!transactions.Any())
            return Result.Failure<LedgerSummaryResponse>("Nenhuma transação encontrada para consolidação.");

        var ledgerSummary = new LedgerSummary(command.ReferenceDate);
        ledgerSummary.AddTransactions(transactions);

        ledgerSummaryRepository.Add(ledgerSummary);

        await ledgerSummaryRepository.CommitScope.CommitAsync(cancellationToken);

        logger.LogInformation("Consolidação de {Date} concluída com sucesso.", command.ReferenceDate);
        return Result.Success(new LedgerSummaryResponse(ledgerSummary.ReferenceDate, ledgerSummary.Balance, ledgerSummary.TotalCredits, ledgerSummary.TotalDebits));
    }
}

public record ConsolidateLedgerCommand(DateTime ReferenceDate);