using System.Text.Json;
using System.Text.Json.Serialization;
using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Aplicacao.DTOs;

public sealed class CriarClienteRequest
{
    public Guid? Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public TipoPessoa TipoPessoa { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Observacoes { get; set; }
    public List<EnderecoRequest> Enderecos { get; set; } = new();
    public List<ContatoRequest> Contatos { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}
