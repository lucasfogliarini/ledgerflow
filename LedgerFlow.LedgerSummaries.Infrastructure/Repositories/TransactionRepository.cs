using LedgerFlow.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.LedgerSummaries.Infrastructure.Repositories;

public class TransactionRepository(LedgerSummariesDbContext dbContext) : Repository(dbContext), ITransactionRepository
{
    public void Add(Transaction transaction)
    {
        dbContext.Add(transaction);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(DateOnly referenceDate, CancellationToken cancellationToken = default)
    {
        var transactions = await Set<Transaction>().Where(t=> DateOnly.FromDateTime(t.CreatedAt).Equals(referenceDate)).ToListAsync(cancellationToken);
        return transactions;
    }
}
