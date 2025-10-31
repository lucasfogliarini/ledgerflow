using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Repositories;

internal class TransactionRepository(LedgerFlowDbContext dbContext) : Repository(dbContext), ITransactionRepository
{
    public void Add(Transaction transaction)
    {
        Add(transaction);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime referenceDate, CancellationToken cancellationToken = default)
    {
        var transactions = await Set<Transaction>().Where(t=> t.CreatedAt == referenceDate).ToListAsync(cancellationToken);
        return transactions;
    }
}
