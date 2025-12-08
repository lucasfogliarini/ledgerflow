using LedgerFlow.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.LedgerSummaries.Infrastructure.Repositories;

internal class LedgerSummaryRepository(LedgerSummariesDbContext dbContext) : Repository(dbContext), ILedgerSummaryRepository
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
