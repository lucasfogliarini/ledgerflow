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

    public TransactionType Type { get; private set; }
    public decimal Value { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; private set; } = DateTime.Now;    

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

    private static Transaction CreateTransaction(TransactionType type, decimal value, string description)
    {
        var transaction = new Transaction(
            type: type,
            value: value,
            description: description
        );
        transaction.AddDomainEvent(new TransactionCreated(transaction.Type, transaction.Value, transaction.Description, transaction.CreatedAt));
        return transaction;
    }
}
