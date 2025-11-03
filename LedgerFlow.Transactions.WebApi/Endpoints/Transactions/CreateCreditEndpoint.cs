using LedgerFlow.Application.Transactions;
using LedgerFlow.Application;

namespace LedgerFlow.LedgerSummaries.WebApi.Endpoints;

internal sealed class CreateCreditEndpoint : IEndpoint
{
    public async Task<IResult> CreateCreditAsync(
        CreateCreditRequest request,
        ICommandHandler<CreateCreditCommand, Transaction> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCreditCommand(request.Value, request.Description);
        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.Ok(result.Value);
    }

    public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder app)
    {
        return app.MapPost($"{Routes.Transactions}/credit", CreateCreditAsync)
           .WithTags(Routes.Transactions)
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest)
           .WithSummary("Cria uma nova transação de crédito.");
    }
}

internal sealed record CreateCreditRequest(decimal Value, string Description);
