using CadastroCliente.Aplicacao.DTOs;

namespace CadastroCliente.Aplicacao.Interfaces;

public interface IAuditoriaAppService
{
    Task<IReadOnlyList<AuditoriaDto>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
