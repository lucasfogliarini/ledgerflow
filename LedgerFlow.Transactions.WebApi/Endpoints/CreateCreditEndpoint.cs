using CSharpFunctionalExtensions;
using LedgerFlow.Transactions.Application;
using Wolverine;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace LedgerFlow.Transactions.WebApi.Endpoints;

internal sealed class CreateCreditEndpoint : IEndpoint
{
    public async Task<IResult> CreateCreditAsync(
        CreateCreditRequest request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCreditCommand(request.Value, request.Description);
        var result = await bus.InvokeAsync<Result<Transaction>>(command, cancellationToken);

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
