using System.Security.Claims;

namespace LedgerFlow.LedgerSummaries.WebApi.Endpoints;

public static class DistributedCache
{
    public static string GetLedgerSummariesKey(HttpContext httpContext, DateTime referenceDate)
    {
        var userId = httpContext.User.FindFirstValue("sid");
        var cacheKey = $"ledger-summaries:{referenceDate:yyyy-MM-dd}-sid:{userId}";
        return cacheKey;
    }
}
