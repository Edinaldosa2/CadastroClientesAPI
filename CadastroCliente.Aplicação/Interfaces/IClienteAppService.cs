using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Domain.Query;

namespace CadastroCliente.Aplicacao.Interfaces;

public interface IClienteAppService
{
    Task<PagedResult<ClienteListItemDto>> ListarAsync(ClienteFiltro filtro, CancellationToken cancellationToken = default);
    Task<ClienteDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClienteDto> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default);
    Task<ClienteDto> CriarAsync(CriarClienteRequest request, CancellationToken cancellationToken = default);
    Task<ClienteDto> AtualizarAsync(Guid id, AtualizarClienteRequest request, CancellationToken cancellationToken = default);
    Task<ClienteDto> PatchAsync(Guid id, PatchClienteRequest request, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClienteDto> RestaurarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClienteDto> AtivarAsync(Guid id, AlterarStatusRequest? request = null, CancellationToken cancellationToken = default);
    Task<ClienteDto> InativarAsync(Guid id, AlterarStatusRequest request, CancellationToken cancellationToken = default);
    Task<ClienteDto> BloquearAsync(Guid id, AlterarStatusRequest request, CancellationToken cancellationToken = default);
}
