namespace LedgerFlow.LedgerSummaries.WebApi;

public interface IEndpoint
{
    IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder app);
}