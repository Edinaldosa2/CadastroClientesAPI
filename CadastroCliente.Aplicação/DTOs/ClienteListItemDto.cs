using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Aplicacao.DTOs;

public sealed class ClienteListItemDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? NomeFantasia { get; init; }
    public TipoPessoa TipoPessoa { get; init; }
    public string Documento { get; init; } = string.Empty;
    public string DocumentoFormatado { get; init; } = string.Empty;
    public StatusCliente Status { get; init; }
    public string? Cidade { get; init; }
    public string? Uf { get; init; }
    public DateTimeOffset CriadoEm { get; init; }
}
