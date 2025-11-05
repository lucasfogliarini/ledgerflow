using LedgerFlow.Application.Transactions;
using LedgerFlow.Application;

namespace LedgerFlow.Transactions.WebApi.Endpoints;

internal sealed class CreateDebitEndpoint : IEndpoint
{
    public async Task<IResult> CreateDebitAsync(
        CreateDebitRequest request,
        ICommandHandler<CreateDebitCommand, Transaction> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateDebitCommand(request.Value, request.Description);
        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.Ok(result.Value);
    }

    public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder app)
    {
        return app.MapPost($"{Routes.Transactions}/debit", CreateDebitAsync)
           .WithTags(Routes.Transactions)
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest)
           .WithSummary("Cria uma nova transação de débito.");
    }
}

internal sealed record CreateDebitRequest(decimal Value, string Description);
