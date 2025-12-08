namespace LedgerFlow;

/// <summary>
/// Representa a raiz de um agregado, sendo a única entrada para modificar o estado interno do agregado.
/// É responsável por garantir as invariantes do domínio.
/// Cor no EventStorming: <b>Roxo</b>.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
