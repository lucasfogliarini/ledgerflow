namespace LedgerFlow.Tests.Unit;

public class TransactionUnitTests
{
    [Theory(DisplayName = "Criação de crédito deve criar transação corretamente quando valores forem válidos")]
    [InlineData(100.00, "Venda de produto")]
    [InlineData(250.50, "Serviço de manutenção")]
    [InlineData(9999.99, "Venda corporativa")]
    public void CreateCredit_ShouldCreateTransaction_WhenValuesAreValid(decimal value, string description)
    {
        // Act
        var result = Transaction.CreateCredit(value, description);

        // Assert
        Assert.True(result.IsSuccess);
        var transaction = result.Value;

        Assert.NotNull(transaction);
        Assert.Equal(TransactionType.Credit, transaction.Type);
        Assert.Equal(value, transaction.Value);
        Assert.Equal(description, transaction.Description);
        Assert.True((DateTime.Now - transaction.CreatedAt).TotalSeconds < 2);
    }

    [Theory(DisplayName = "Criação de débito deve criar transação corretamente quando valores forem válidos")]
    [InlineData(500.00, "Compra de insumos")]
    [InlineData(10.25, "Tarifa bancária")]
    [InlineData(999.00, "Pagamento de fornecedor")]
    public void CreateDebit_ShouldCreateTransaction_WhenValuesAreValid(decimal value, string description)
    {
        // Act
        var result = Transaction.CreateDebit(value, description);

        // Assert
        Assert.True(result.IsSuccess);
        var transaction = result.Value;

        Assert.NotNull(transaction);
        Assert.Equal(TransactionType.Debit, transaction.Type);
        Assert.Equal(value, transaction.Value);
        Assert.Equal(description, transaction.Description);
        Assert.True((DateTime.Now - transaction.CreatedAt).TotalSeconds < 2);
    }

    [Theory(DisplayName = "Criação de crédito deve falhar quando valor for inválido")]
    [InlineData(0)]
    [InlineData(-500)]
    public void CreateCredit_ShouldFail_WhenValueIsInvalid(decimal value)
    {
        // Act
        var result = Transaction.CreateCredit(value, "Crédito inválido");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("maior que zero", result.Error);
    }

    [Theory(DisplayName = "Criação de débito deve falhar quando valor for inválido")]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateDebit_ShouldFail_WhenValueIsInvalid(decimal value)
    {
        // Act
        var result = Transaction.CreateDebit(value, "Débito inválido");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("maior que zero", result.Error);
    }
}
