using Microsoft.EntityFrameworkCore;
using System.Reflection;
using LedgerFlow.Infrastructure;
using Wolverine;

namespace LedgerFlow.LedgerSummaries.Infrastructure;

internal class LedgerSummariesDbContext(IMessageBus bus, DbContextOptions options) : LedgerFlowDbContext(bus, options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        modelBuilder.ApplyConfigurationsFromAssembly(thisAssembly);
    }
}
