using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Aplicacao.DTOs;

public sealed class ClienteDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? NomeFantasia { get; init; }
    public TipoPessoa TipoPessoa { get; init; }
    public string Documento { get; init; } = string.Empty;
    public string DocumentoFormatado { get; init; } = string.Empty;
    public string? InscricaoEstadual { get; init; }
    public DateTime? DataNascimento { get; init; }
    public StatusCliente Status { get; init; }
    public string? MotivoStatus { get; init; }
    public string? Observacoes { get; init; }
    public bool Excluido { get; init; }
    public DateTimeOffset CriadoEm { get; init; }
    public DateTimeOffset? AtualizadoEm { get; init; }
    public IReadOnlyList<EnderecoDto> Enderecos { get; init; } = Array.Empty<EnderecoDto>();
    public IReadOnlyList<ContatoDto> Contatos { get; init; } = Array.Empty<ContatoDto>();
}
