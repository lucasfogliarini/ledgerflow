namespace LedgerFlow.Transactions.WebApi;

public interface IEndpoint
{
    IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder app);
}