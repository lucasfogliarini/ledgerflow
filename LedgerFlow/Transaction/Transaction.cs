using CSharpFunctionalExtensions;
using System.Transactions;

namespace LedgerFlow;

public class Transaction : AggregateRoot, IAuditable
{
    public Transaction() { }
    private Transaction(TransactionType type, decimal value, string description)
    {
        Type = type;
        Value = value;
        Description = description;
    }

    private Transaction(TransactionType type, decimal value, DateTime createdAt)
    {
        Type = type;
        Value = value;
        CreatedAt = createdAt;
    }

    public TransactionType Type { get; }
    public decimal Value { get; }
    public string? Description { get; }
    public DateTime CreatedAt { get; } = DateTime.Now;
    public DateTime? UpdatedAt { get; } = DateTime.Now;    

    /// <summary>
    /// Cria uma transação de crédito.
    /// </summary>
    public static Result<Transaction> CreateCredit(decimal value, string description)
    {
        if (value <= 0)
            return Result.Failure<Transaction>("O valor do crédito deve ser maior que zero.");

        return CreateTransaction(TransactionType.Credit, value, description);
    }

    /// <summary>
    /// Cria uma transação de débito.
    /// </summary>
    public static Result<Transaction> CreateDebit(decimal value, string description)
    {
        if (value <= 0)
            return Result.Failure<Transaction>("O valor do débito deve ser maior que zero.");

        return CreateTransaction(TransactionType.Debit, value, description);
    }

    public static Transaction CreateTransaction(TransactionType type, decimal value, DateTime createdAt)
    {
        return new Transaction(type, value, createdAt);
    }

    private static Transaction CreateTransaction(TransactionType type, decimal value, string description)
    {
        var transaction = new Transaction(
            type: type,
            value: value,
            description: description
        );
        transaction.AddDomainEvent(new TransactionCreated(transaction.Type, transaction.Value, transaction.CreatedAt));
        return transaction;
    }
}
