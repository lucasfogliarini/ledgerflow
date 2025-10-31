using LedgerFlow.Application.Transactions;
using LedgerFlow.Application;

namespace LedgerFlow.WebApi.Endpoints.Transactions;

internal sealed class CreateCreditEndpoint : IEndpoint
{
    public async Task<IResult> CreateCreditAsync(
        CreateCreditTransactionRequest request,
        ICommandHandler<CreateCreditCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCreditCommand(request.Value, request.Description);
        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.Ok();
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{Routes.Transactions}/credit", CreateCreditAsync)
           .WithTags(Routes.Transactions)
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest)
           .WithSummary("Cria uma nova transação de crédito.");
    }
}

internal sealed record CreateCreditTransactionRequest(decimal Value, string Description);
