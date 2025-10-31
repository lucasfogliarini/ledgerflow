namespace LedgerFlow;

public interface ILedgerSummaryRepository : IRepository
{
    void Add(LedgerSummary ledgerSummary);
}
