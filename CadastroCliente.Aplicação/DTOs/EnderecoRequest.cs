using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Aplicacao.DTOs;

public sealed class EnderecoRequest
{
    public TipoEndereco Tipo { get; set; } = TipoEndereco.Residencial;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string? Pais { get; set; }
    public bool Principal { get; set; }
}
