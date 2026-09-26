namespace CadastroCliente.Aplicacao.DTOs;

public sealed class TokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public string Perfil { get; init; } = string.Empty;
}
