using LedgerFlow;
using LedgerFlow.Transactions.Infrastructure;
using LedgerFlow.Transactions.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddDbContext<TransactionsDbContext>("TransactionsDatabase");        
        builder.Services.AddRepositories();
        builder.AddOpenTelemetryExporter();
        builder.AddRateLimiter();
        builder.AddLivenessHealthCheck();
    }
    public static void Migrate(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

        if(db.Database.IsRelational())
            db.Database.Migrate();
    }
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<ITransactionRepository, TransactionRepository>();
    }
}
