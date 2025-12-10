using Microsoft.EntityFrameworkCore;
using System.Reflection;
using LedgerFlow.Infrastructure;

namespace LedgerFlow.LedgerSummaries.Infrastructure;

internal class LedgerSummariesDbContext(DbContextOptions options) : LedgerFlowDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        modelBuilder.ApplyConfigurationsFromAssembly(thisAssembly);
    }
}
