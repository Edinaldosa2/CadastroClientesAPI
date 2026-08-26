using CadastroCliente.Aplicacao.DTOs;

namespace CadastroCliente.Aplicacao.Interfaces;

public interface IContatoAppService
{
    Task<IReadOnlyList<ContatoDto>> ListarAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<ContatoDto> ObterAsync(Guid clienteId, Guid contatoId, CancellationToken cancellationToken = default);
    Task<ContatoDto> CriarAsync(Guid clienteId, ContatoRequest request, CancellationToken cancellationToken = default);
    Task<ContatoDto> AtualizarAsync(Guid clienteId, Guid contatoId, ContatoRequest request, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid clienteId, Guid contatoId, CancellationToken cancellationToken = default);
}
