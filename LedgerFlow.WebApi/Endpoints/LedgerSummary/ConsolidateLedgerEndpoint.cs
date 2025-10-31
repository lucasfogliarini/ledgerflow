using LedgerFlow.Application;
using LedgerFlow.Application.LedgerSummaries;

namespace LedgerFlow.WebApi.Endpoints.Transactions;

internal sealed class ConsolidateLedgerEndpoint : IEndpoint
{
    public async Task<IResult> ConsolidateLedgerAsync(
        ConsolidateLedgerCommandRequest request,
        ICommandHandler<ConsolidateLedgerCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new ConsolidateLedgerCommand(request.ReferenceDate);
        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.Ok();
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{Routes.LedgerSummaries}/consolidate", ConsolidateLedgerAsync)
           .WithTags(Routes.LedgerSummaries)
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest);
    }
}

internal sealed record ConsolidateLedgerCommandRequest(DateTime ReferenceDate);
