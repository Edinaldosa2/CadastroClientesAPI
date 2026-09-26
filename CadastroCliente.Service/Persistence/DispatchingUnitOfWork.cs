using CadastroCliente.Aplicacao.Abstractions;
using CadastroCliente.Data.Context;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Interfaces;

namespace CadastroCliente.Service.Persistence;

public sealed class DispatchingUnitOfWork : IUnitOfWork
{
    private readonly CadastroClienteContext _context;
    private readonly IDomainEventDispatcher _dispatcher;

    public DispatchingUnitOfWork(CadastroClienteContext context, IDomainEventDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    public void RegisterNew<T>(T entity) where T : EntityBase => _context.RegisterNew(entity);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var saved = await _context.SaveChangesAsync(cancellationToken);
        var events = _context.ColetarEventos();
        if (events.Count > 0)
        {
            await _dispatcher.DispatchAsync(events, cancellationToken);
            saved += await _context.SaveChangesAsync(cancellationToken);
        }

        return saved;
    }
}
