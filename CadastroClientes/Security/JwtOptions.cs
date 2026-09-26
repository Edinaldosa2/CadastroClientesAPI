namespace CadastroClientes.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "cadastro-clientes";
    public string Audience { get; set; } = "cadastro-clientes";
    public string Key { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 60;
}

public sealed class AuthUser
{
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Perfil { get; set; } = "leitura";
}
