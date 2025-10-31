namespace LedgerFlow;

public class Transaction
{
    public Guid Id { get; }
    public TransactionType Type { get; }
    public decimal Value { get; }
    public string Description { get; }
    public DateTime CreatedAt { get; }

    private Transaction(Guid id, TransactionType type, decimal value, string description, DateTime createdAt)
    {
        Id = id;
        Type = type;
        Value = value;
        Description = description;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Cria uma transação de crédito.
    /// </summary>
    public static Transaction CreateCredit(decimal value, string description)
    {
        if (value <= 0)
            throw new ArgumentException("O valor do crédito deve ser maior que zero.", nameof(value));

        return new Transaction(
            id: Guid.NewGuid(),
            type: TransactionType.Credit,
            value: value,
            description: description,
            createdAt: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Cria uma transação de débito.
    /// </summary>
    public static Transaction CreateDebit(decimal value, string description)
    {
        if (value <= 0)
            throw new ArgumentException("O valor do débito deve ser maior que zero.", nameof(value));

        return new Transaction(
            id: Guid.NewGuid(),
            type: TransactionType.Debit,
            value: value,
            description: description,
            createdAt: DateTime.UtcNow
        );
    }

    public override string ToString() =>  $"{Type} {Value:C} - {Description} ({CreatedAt:yyyy-MM-dd HH:mm})";
}
