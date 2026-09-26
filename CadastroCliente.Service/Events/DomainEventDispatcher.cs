using CadastroCliente.Aplicacao.Abstractions;
using CadastroCliente.Domain.Events;

namespace CadastroCliente.Service.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IEnumerable<IDomainEventHandler> _handlers;

    public DomainEventDispatcher(IEnumerable<IDomainEventHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            foreach (var handler in _handlers)
            {
                await handler.HandleAsync(domainEvent, cancellationToken);
            }
        }
    }
}
