using LedgerFlow.Application;
using LedgerFlow.Application.LedgerSummaries;

namespace LedgerFlow.LedgerSummaries.WebApi.Endpoints;

internal sealed class ConsolidateLedgerEndpoint : IEndpoint
{
    public async Task<IResult> ConsolidateLedgerAsync(
        ConsolidateLedgerRequest request,
        ICommandHandler<ConsolidateLedgerCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new ConsolidateLedgerCommand(request.ReferenceDate);
        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.Ok();
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
