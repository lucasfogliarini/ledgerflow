namespace LedgerFlow;

/// <summary>
/// Representa um evento de domínio que indica algo relevante que aconteceu no passado no domínio.
/// Pode ser utilizado para comunicação entre contextos limitados (bounded contexts) ou para processamento assíncrono.
/// Cor no EventStorming: <b>Laranja</b>.
/// </summary>
public interface IDomainEvent;
