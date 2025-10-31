using LedgerFlow.Application.Transactions;
using LedgerFlow.Application;

namespace LedgerFlow.WebApi.Endpoints.Transactions;

internal sealed class CreateDebitEndpoint : IEndpoint
{
    public async Task<IResult> CreateDebitAsync(
        CreateDebitRequest request,
        ICommandHandler<CreateDebitCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateDebitCommand(request.Value, request.Description);
        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.Ok();
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{Routes.Transactions}/debit", CreateDebitAsync)
           .WithTags(Routes.Transactions)
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest)
           .WithSummary("Cria uma nova transação de débito.");
    }
}

internal sealed record CreateDebitRequest(decimal Value, string Description);
