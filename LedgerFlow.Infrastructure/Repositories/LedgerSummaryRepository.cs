
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Repositories;

internal class LedgerSummaryRepository(LedgerFlowDbContext dbContext) : Repository(dbContext), ILedgerSummaryRepository
{
    public void Add(LedgerSummary ledgerSummary)
    {
        dbContext.Add(ledgerSummary);
    }

    public async Task<LedgerSummary> GetAsync(DateTime ReferenceDate, CancellationToken cancellationToken = default)
    {
        return await Set<LedgerSummary>().Include(e=>e.Transactions).FirstOrDefaultAsync(e=>e.ReferenceDate.Date ==  ReferenceDate.Date, cancellationToken);
    }
}
