using CSharpFunctionalExtensions;

namespace LedgerFlow;

public class LedgerSummary(DateTime referenceDate) : AggregateRoot, IAuditable
{
    public DateTime ReferenceDate { get; set; } = referenceDate;
    public IList<Transaction> Transactions { get; private set; } = [];
    public decimal TotalCredits { get; private set; }
    public decimal TotalDebits { get; private set; }
    public decimal Balance => TotalCredits - TotalDebits;    

    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime UpdatedAt { get; private set; } = DateTime.Now;

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);

        if (transaction.Type == TransactionType.Credit)
            TotalCredits += transaction.Value;
        else
            TotalDebits += transaction.Value;
    }

    public Result AddTransactions(IEnumerable<Transaction> transactions)
    {
        foreach (var t in transactions)
            AddTransaction(t);

        return Result.Success();
    }

    public override string ToString() => $"Consolidado para {CreatedAt:yyyy-MM-dd}: +{TotalCredits:C} -{TotalDebits:C} = {Balance:C}";
}
