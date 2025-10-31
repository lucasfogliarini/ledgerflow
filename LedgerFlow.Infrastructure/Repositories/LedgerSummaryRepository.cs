namespace LedgerFlow.Infrastructure.Repositories;

internal class LedgerSummaryRepository(LedgerFlowDbContext dbContext) : Repository(dbContext), ILedgerSummaryRepository
{
    public void Add(LedgerSummary ledgerSummary)
    {
        dbContext.Add(ledgerSummary);
    }
}
