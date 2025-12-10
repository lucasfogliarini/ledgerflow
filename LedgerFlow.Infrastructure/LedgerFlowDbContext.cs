using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure;

public abstract class LedgerFlowDbContext(DbContextOptions options) : DbContext(options), ICommitScope
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    public int Commit()
    {
        return base.SaveChanges();
    }
}
