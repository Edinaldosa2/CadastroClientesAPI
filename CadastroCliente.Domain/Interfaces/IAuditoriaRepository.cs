using CadastroCliente.Domain.Query;

namespace CadastroCliente.Domain.Interfaces;

public interface IAuditoriaRepository
{
    Task AddAsync(AuditoriaItem item, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditoriaItem>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
