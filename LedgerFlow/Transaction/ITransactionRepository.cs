namespace LedgerFlow;

public interface ITransactionRepository : IRepository
{
    void Add(Transaction transaction);
    Task<IEnumerable<Transaction>> GetTransactionsAsync(DateOnly referenceDate, CancellationToken cancellationToken = default);
}
