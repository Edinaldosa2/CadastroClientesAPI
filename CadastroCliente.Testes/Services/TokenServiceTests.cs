using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using CadastroCliente.Aplicacao.Abstractions;
using CadastroCliente.Aplicacao.DTOs;
using CadastroClientes.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CadastroCliente.Testes.Services;

public class TokenServiceTests
{
    [Theory]
    [InlineData("admin", new[] { "admin", "escrita", "leitura" })]
    [InlineData("escrita", new[] { "leitura", "escrita" })]
    [InlineData("leitura", new[] { "leitura" })]
    public void RolesDoPerfil_mapeia_hierarquia(string perfil, string[] esperados)
    {
        TokenService.RolesDoPerfil(perfil).Should().Equal(esperados);
    }

    [Fact]
    public void ListarUsuarios_nunca_expoe_senha()
    {
        var service = Criar();
        var usuarios = service.ListarUsuarios();
        usuarios.Should().Contain(u => u.Usuario == "admin" && u.Perfil == Perfis.Admin);
        var json = JsonSerializer.Serialize(usuarios);
        json.Should().NotContain("senha");
        json.Should().NotContain("admin-dev");
        json.Should().NotContain("Senha");
    }

    [Fact]
    public void Emitir_admin_inclui_papeis_completos()
    {
        var service = Criar();
        var token = service.Emitir(new LoginRequest { Usuario = "admin", Senha = "admin-dev" });
        token.Perfil.Should().Be(Perfis.Admin);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .Should().BeEquivalentTo(new[] { Perfis.Admin, Perfis.Escrita, Perfis.Leitura });
    }

    private static TokenService Criar()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Users:0:Usuario"] = "admin",
                ["Users:0:Senha"] = "admin-dev",
                ["Users:0:Perfil"] = "admin",
                ["Users:1:Usuario"] = "editor",
                ["Users:1:Senha"] = "editor-dev",
                ["Users:1:Perfil"] = "escrita"
            })
            .Build();

        return new TokenService(Options.Create(new JwtOptions
        {
            Issuer = "cadastro-clientes-test",
            Audience = "cadastro-clientes-test",
            Key = "testing-key-not-for-production-32ch",
            ExpiresMinutes = 60
        }), config);
    }
}
