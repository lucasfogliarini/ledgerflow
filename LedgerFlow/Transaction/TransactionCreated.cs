namespace LedgerFlow;

public record TransactionCreated(
    TransactionType Type,
    decimal Value,
    string Description,
    DateTime CreatedAt) : IDomainEvent;
