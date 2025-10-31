namespace LedgerFlow.Tests.Unit;

public class LedgerSummaryUnitTests
{
    [Fact(DisplayName = "Adicionar transações deve adicionar uma transação quando lista for nula")]
    public void AddTransaction_ShouldAddTransaction_WhenTransactionIsValid()
    {
        // Arrange
        var summary = new LedgerSummary(DateTime.Now);
        var credit = Transaction.CreateCredit(500m, "Venda no cartão");
        var debit = Transaction.CreateDebit(100m, "Compra de material");

        // Act
        summary.AddTransaction(credit.Value);
        summary.AddTransaction(debit.Value);

        // Assert
        Assert.Equal(2, summary.Transactions.Count);
        Assert.Equal(500m, summary.TotalCredits);
        Assert.Equal(100m, summary.TotalDebits);
        Assert.Equal(400m, summary.Balance);
    }

    [Fact(DisplayName = "Adicionar transações deve adicionar múltiplas transações quando lista for válida")]
    public void AddTransactions_ShouldAddMultipleTransactions_WhenListIsValid()
    {
        // Arrange
        var summary = new LedgerSummary(DateTime.Now);

        var transactions = new List<Transaction>
        {
            Transaction.CreateCredit(100m, "Venda 1").Value,
            Transaction.CreateCredit(50m, "Venda 2").Value,
            Transaction.CreateDebit(30m, "Compra 1").Value
        };

        // Act
        summary.AddTransactions(transactions);

        // Assert
        Assert.Equal(3, summary.Transactions.Count);
        Assert.Equal(150m, summary.TotalCredits);
        Assert.Equal(30m, summary.TotalDebits);
        Assert.Equal(120m, summary.Balance);
    }
}
