using CadastroCliente.Aplicacao.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace CadastroClientes.Security;

public static class AppPolicies
{
    public const string Leitura = "Leitura";
    public const string Escrita = "Escrita";
    public const string Admin = "Admin";

    public static void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(Leitura, policy => policy.RequireRole(Perfis.Leitura, Perfis.Escrita, Perfis.Admin));
        options.AddPolicy(Escrita, policy => policy.RequireRole(Perfis.Escrita, Perfis.Admin));
        options.AddPolicy(Admin, policy => policy.RequireRole(Perfis.Admin));
    }
}
