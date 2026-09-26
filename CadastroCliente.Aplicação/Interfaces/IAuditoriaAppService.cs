using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Domain.Query;

namespace CadastroCliente.Aplicacao.Interfaces;

public interface IAuditoriaAppService
{
    Task<IReadOnlyList<AuditoriaDto>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditoriaDto>> ListarGlobalAsync(int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);
}
