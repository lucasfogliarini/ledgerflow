using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Wolverine;

namespace LedgerFlow.Infrastructure;

internal class LedgerFlowDbContext(IMessageBus bus, DbContextOptions options) : DbContext(options), ICommitScope
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        modelBuilder.ApplyConfigurationsFromAssembly(thisAssembly);
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEvents();

        return result;
    }

    private async Task PublishDomainEvents()
    {
        var aggregates = ChangeTracker
            .Entries()
            .Select(x => x.Entity)
            .OfType<AggregateRoot>();

        var eventMessages = aggregates
            .SelectMany(e => e.DomainEvents);

        foreach (var eventMessage in eventMessages)
        {
            await bus.PublishAsync(eventMessage);
        }

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();
    }

    public int Commit()
    {
        return base.SaveChanges();
    }
}
