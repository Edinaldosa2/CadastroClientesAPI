using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Aplicacao.DTOs;

public sealed class EnderecoDto
{
    public Guid Id { get; init; }
    public TipoEndereco Tipo { get; init; }
    public string Logradouro { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string? Complemento { get; init; }
    public string Bairro { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Uf { get; init; } = string.Empty;
    public string Cep { get; init; } = string.Empty;
    public string CepFormatado { get; init; } = string.Empty;
    public string Pais { get; init; } = string.Empty;
    public bool Principal { get; init; }
}
