using Azure;
using LedgerFlow.Application;
using LedgerFlow.Application.LedgerSummaries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

namespace LedgerFlow.LedgerSummaries.WebApi.Endpoints;

internal sealed class GetLedgerSummariesEndpoint : IEndpoint
{
    public async Task<IResult> GetLedgerSummariesAsync(
        [FromBody] GetLedgerSummaryRequest request,
        IQueryHandler<GetLedgerSummariesQuery, GetLedgerSummariesResponse> handler,
        IDistributedCache cache,
        ILogger<GetLedgerSummariesEndpoint> logger,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Results.BadRequest("O corpo da requisição não pode ser nulo.");

        var cacheKey = DistributedCache.GetLedgerSummariesKey(httpContext, request.ReferenceDate);

        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogInformation("Cache hit para {CacheKey}", cacheKey);
            var cachedResponse = JsonSerializer.Deserialize<GetLedgerSummariesResponse>(cached);
            httpContext.Response.Headers.Append("X-Cache", "HIT");
            return Results.Ok(cachedResponse);
        }

        var query = new GetLedgerSummariesQuery(request.ReferenceDate);
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result.Value),
            cancellationToken);

        httpContext.Response.Headers.Append("X-Cache", "MISS");
        logger.LogInformation("Cache salvo para {CacheKey}", cacheKey);

        return Results.Ok(result.Value);
    }

    public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder app)
    {
        return app.MapGet($"{Routes.LedgerSummaries}", GetLedgerSummariesAsync)
           .WithTags(Routes.LedgerSummaries)
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest);
    }
}

internal sealed record GetLedgerSummaryRequest(DateTime ReferenceDate);
