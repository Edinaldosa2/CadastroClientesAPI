using CadastroCliente.Aplicacao.DTOs;

namespace CadastroCliente.Aplicacao.Interfaces;

public interface IEnderecoAppService
{
    Task<IReadOnlyList<EnderecoDto>> ListarAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<EnderecoDto> ObterAsync(Guid clienteId, Guid enderecoId, CancellationToken cancellationToken = default);
    Task<EnderecoDto> CriarAsync(Guid clienteId, EnderecoRequest request, CancellationToken cancellationToken = default);
    Task<EnderecoDto> AtualizarAsync(Guid clienteId, Guid enderecoId, EnderecoRequest request, CancellationToken cancellationToken = default);
    Task<EnderecoDto> DefinirPrincipalAsync(Guid clienteId, Guid enderecoId, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid clienteId, Guid enderecoId, CancellationToken cancellationToken = default);
}
