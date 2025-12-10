using LedgerFlow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LedgerFlow.Transactions.Infrastructure;

public class TransactionsDbContext(DbContextOptions options) : LedgerFlowDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        modelBuilder.ApplyConfigurationsFromAssembly(thisAssembly);
    }
}
