using Microsoft.EntityFrameworkCore;
using System.Reflection;
using LedgerFlow.Infrastructure;
using Wolverine;

namespace LedgerFlow.Transactions.Infrastructure;

internal class TransactionsDbContext(DbContextOptions options) : LedgerFlowDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        modelBuilder.ApplyConfigurationsFromAssembly(thisAssembly);
    }
}
