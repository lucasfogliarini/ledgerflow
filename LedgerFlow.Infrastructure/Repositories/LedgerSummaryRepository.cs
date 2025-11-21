using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Repositories;

internal class LedgerSummaryRepository(LedgerFlowDbContext dbContext) : Repository(dbContext), ILedgerSummaryRepository
{
    public void Add(LedgerSummary ledgerSummary)
    {
        dbContext.Add(ledgerSummary);
    }

    public async Task<IEnumerable<LedgerSummary>> GetAsync(DateTime referenceDate, CancellationToken cancellationToken = default)
    {
        return await Set<LedgerSummary>()
                    .Where(e=>e.ReferenceDate.Date ==  referenceDate.Date)
                    .OrderByDescending(e=>e.ReferenceDate)
                    .ToListAsync(cancellationToken);
    }
}
