using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Query;

namespace CadastroCliente.Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(Guid id, bool incluirExcluidos = false, CancellationToken cancellationToken = default);
    Task<Cliente?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default);
    Task<bool> ExistsDocumentoAsync(string documento, Guid? ignoreId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsDocumentoAtivoAsync(string documento, Guid? ignoreId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Cliente>> SearchAsync(ClienteFiltro filtro, CancellationToken cancellationToken = default);
    Task<ClienteEstatisticas> GetEstatisticasAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default);
    void Remove(Cliente cliente);
}
