namespace LedgerFlow.Tests.Unit;

public class TransactionUnitTests
{
    [Theory(DisplayName = "Criação de crédito deve criar transação corretamente quando valores forem válidos")]
    [InlineData(100.00, "Venda de produto")]
    [InlineData(250.50, "Serviço de manutenção")]
    [InlineData(9999.99, "Venda corporativa")]
    public void CreateCredit_ShouldCreateTransaction_WhenValuesAreValid(decimal value, string description)
    {
        // Arrange e Act
        var transaction = Transaction.CreateCredit(value, description);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(TransactionType.Credit, transaction.Type);
        Assert.Equal(value, transaction.Value);
        Assert.Equal(description, transaction.Description);
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.True((DateTime.UtcNow - transaction.CreatedAt).TotalSeconds < 2);
    }

    [Theory(DisplayName = "Criação de débito deve criar transação corretamente quando valores forem válidos")]
    [InlineData(500.00, "Compra de insumos")]
    [InlineData(10.25, "Tarifa bancária")]
    [InlineData(999.00, "Pagamento de fornecedor")]
    public void CreateDebit_ShouldCreateTransaction_WhenValuesAreValid(decimal value, string description)
    {
        // Arrange e Act
        var transaction = Transaction.CreateDebit(value, description);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(TransactionType.Debit, transaction.Type);
        Assert.Equal(value, transaction.Value);
        Assert.Equal(description, transaction.Description);
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.True((DateTime.UtcNow - transaction.CreatedAt).TotalSeconds < 2);
    }

    [Theory(DisplayName = "Criação de crédito não deve criar transação quando valores forem inválidos")]
    [InlineData(0)]
    [InlineData(-500)]
    public void CreateCredit_ShouldThrowArgumentException_WhenValueIsInvalid(decimal value)
    {
        // Arrange
        string description = "Crédito inválido";

        // Act e Assert
        var ex = Assert.Throws<ArgumentException>(() => Transaction.CreateCredit(value, description));
        Assert.Contains("maior que zero", ex.Message);
    }

    [Theory(DisplayName = "Criação de crédito não deve criar transação quando valores forem inválidos")]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateDebit_ShouldThrowArgumentException_WhenValueIsInvalid(decimal value)
    {
        // Arrange
        string description = "Débito inválido";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Transaction.CreateDebit(value, description));
        Assert.Contains("maior que zero", ex.Message);
    }

    [Theory(DisplayName = "ToString Deve Retornar String Formatada Corretamente")]
    [InlineData(250.75, "Venda via PIX", TransactionType.Credit)]
    [InlineData(99.90, "Compra de insumos", TransactionType.Debit)]
    public void ToString_ShouldReturnFormattedString_WhenTransactionIsValid(decimal value, string description, TransactionType type)
    {
        // Arrange
        var transaction = type == TransactionType.Credit
            ? Transaction.CreateCredit(value, description)
            : Transaction.CreateDebit(value, description);

        // Act
        string result = transaction.ToString();

        // Assert
        Assert.Contains(type.ToString(), result);
        Assert.Contains(description, result);
        Assert.Contains(value.ToString("F2").Replace(".", ","), result);
        Assert.Contains(DateTime.UtcNow.Year.ToString(), result);
    }
}
