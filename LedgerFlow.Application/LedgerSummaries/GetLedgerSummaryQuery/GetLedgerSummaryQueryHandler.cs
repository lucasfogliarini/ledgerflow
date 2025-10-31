using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace LedgerFlow.Application.LedgerSummaries;

public class GetLedgerSummaryQueryHandler(ILedgerSummaryRepository ledgerSummaryRepository, ILogger<GetLedgerSummaryQueryHandler> logger) : IQueryHandler<GetLedgerSummaryQuery, GetLedgerSummaryResponse>
{
    public async Task<Result<GetLedgerSummaryResponse>> HandleAsync(GetLedgerSummaryQuery query, CancellationToken cancellationToken = default)
    {
        if (query is null)
            return Result.Failure<GetLedgerSummaryResponse>("A query não pode ser nula.");

        var ledgerSummary = await ledgerSummaryRepository.GetAsync(query.ReferenceDate, cancellationToken);

        if(ledgerSummary is null)
            return Result.Failure<GetLedgerSummaryResponse>("Nenhum relatório consolidado foi encontrado.");

        return Result.Success(new GetLedgerSummaryResponse(ledgerSummary.ReferenceDate, ledgerSummary.Balance, ledgerSummary.TotalCredits, ledgerSummary.TotalDebits, ledgerSummary.Transactions));
    }
}

public record GetLedgerSummaryQuery(DateTime ReferenceDate) : IQuery<GetLedgerSummaryResponse>;
public record GetLedgerSummaryResponse(DateTime ReferenceDate, decimal Balance, decimal TotalCredits, decimal TotalDebits, IEnumerable<Transaction> Transactions);
