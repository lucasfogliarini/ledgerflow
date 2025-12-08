namespace LedgerFlow.LedgerSummaries.Application;
public record LedgerSummaryResponse(DateTime ReferenceDate, decimal Balance, decimal TotalCredits, decimal TotalDebits);