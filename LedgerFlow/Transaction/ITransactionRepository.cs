namespace LedgerFlow;

public interface ITransactionRepository : IRepository
{
    void Add(Transaction transaction);
    Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime referenceDate, CancellationToken cancellationToken = default);
}
