using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace LedgerFlow.Infrastructure;

internal abstract class LedgerFlowDbContext(IMessageBus bus, DbContextOptions options) : DbContext(options), ICommitScope
{
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
