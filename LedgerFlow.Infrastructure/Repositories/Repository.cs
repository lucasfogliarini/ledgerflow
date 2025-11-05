namespace LedgerFlow.Infrastructure.Repositories
{
    internal abstract class Repository(LedgerFlowDbContext dbContext) : IRepository
    {
        public ICommitScope CommitScope => dbContext;
        public IQueryable<TEntity> Set<TEntity>() where TEntity : Entity
        {
            var query = dbContext.Set<TEntity>();
            return query;
        }
    }
}
