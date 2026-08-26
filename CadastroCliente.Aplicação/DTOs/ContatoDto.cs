using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Aplicacao.DTOs;

public sealed class ContatoDto
{
    public Guid Id { get; init; }
    public TipoContato Tipo { get; init; }
    public string Valor { get; init; } = string.Empty;
    public string ValorFormatado { get; init; } = string.Empty;
    public string? Nome { get; init; }
    public bool Principal { get; init; }
}
