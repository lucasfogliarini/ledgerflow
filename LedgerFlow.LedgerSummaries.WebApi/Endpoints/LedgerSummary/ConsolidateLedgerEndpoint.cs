using LedgerFlow.Application;
using LedgerFlow.Application.LedgerSummaries;
using Microsoft.Extensions.Caching.Distributed;

namespace LedgerFlow.LedgerSummaries.WebApi.Endpoints;

internal sealed class ConsolidateLedgerEndpoint : IEndpoint
{
    public async Task<IResult> ConsolidateLedgerAsync(
        ConsolidateLedgerRequest request,
        ICommandHandler<ConsolidateLedgerCommand, LedgerSummaryResponse> handler,
        IDistributedCache cache,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var command = new ConsolidateLedgerCommand(request.ReferenceDate);
        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        var cacheKey = DistributedCache.GetLedgerSummariesKey(httpContext, request.ReferenceDate);
        await cache.RemoveAsync(cacheKey);        
        return Results.Ok(result.Value);
    }

    public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder app)
    {
        return app.MapPost($"{Routes.LedgerSummaries}/consolidate", ConsolidateLedgerAsync)
           .WithTags(Routes.LedgerSummaries)
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest);
    }
}

internal sealed record ConsolidateLedgerRequest(DateTime ReferenceDate);
