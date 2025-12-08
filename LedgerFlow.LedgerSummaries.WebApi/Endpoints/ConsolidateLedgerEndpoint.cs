using CSharpFunctionalExtensions;
using LedgerFlow.LedgerSummaries.Application;
using Microsoft.Extensions.Caching.Distributed;
using Wolverine;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace LedgerFlow.LedgerSummaries.WebApi.Endpoints;

internal sealed class ConsolidateLedgerEndpoint : IEndpoint
{
    public async Task<IResult> ConsolidateLedgerAsync(
        ConsolidateLedgerRequest request,
        IMessageBus bus,
        IDistributedCache cache,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var command = new ConsolidateLedgerCommand(request.ReferenceDate);
        var result = await bus.InvokeAsync<Result<LedgerSummaryResponse>>(command, cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        var cacheKey = DistributedCache.GetLedgerSummariesKey(httpContext, request.ReferenceDate);
        await cache.RemoveAsync(cacheKey, cancellationToken);        
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
