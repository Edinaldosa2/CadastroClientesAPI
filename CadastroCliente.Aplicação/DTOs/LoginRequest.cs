namespace CadastroCliente.Aplicacao.DTOs;

public sealed class LoginRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
