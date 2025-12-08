namespace LedgerFlow;

public record TransactionCreated(
    TransactionType Type,
    decimal Value,
    DateTime CreatedAt) : IDomainEvent;
