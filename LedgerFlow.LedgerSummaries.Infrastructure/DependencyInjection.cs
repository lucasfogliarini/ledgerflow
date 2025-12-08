using LedgerFlow;
using LedgerFlow.LedgerSummaries.Infrastructure;
using LedgerFlow.LedgerSummaries.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddDbContext<LedgerSummariesDbContext>("LedgerSummariesDatabase");        
        builder.Services.AddRepositories();
        builder.AddOpenTelemetryExporter();
        builder.AddRateLimiter();
        builder.AddLivenessHealthCheck();
    }
    public static void Migrate(this WebApplication app)
    {
        app.Migrate<LedgerSummariesDbContext>();
    }
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<ITransactionRepository, TransactionRepository>();
        services.AddTransient<ILedgerSummaryRepository, LedgerSummaryRepository>();
    }
}
