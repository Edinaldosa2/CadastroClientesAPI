using System.Text.Json;
using System.Text.Json.Serialization;

namespace CadastroCliente.Aplicacao.DTOs;

public sealed class PatchClienteRequest
{
    public string? Documento { get; set; }
    public string? Nome { get; set; }
    public string? NomeFantasia { get; set; }
    public string? InscricaoEstadual { get; set; }
    public DateTime? DataNascimento { get; set; }
    public bool AtualizarDataNascimento { get; set; }
    public string? Observacoes { get; set; }
    public bool AtualizarObservacoes { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}
