using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Aplicacao.DTOs;

public sealed class AtualizarClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string? InscricaoEstadual { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Observacoes { get; set; }
}
