using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Aplicacao.DTOs;

public sealed class ContatoRequest
{
    public TipoContato Tipo { get; set; } = TipoContato.Email;
    public string Valor { get; set; } = string.Empty;
    public string? Nome { get; set; }
    public bool Principal { get; set; }
}
