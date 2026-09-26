using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CadastroCliente.Testes.Integration;

public class HostSafetyTests
{
    [Fact]
    public void Producao_rejeita_jwt_curta_ou_dev_only()
    {
        FalhaAoSubir("Production", "Jwt:Key", "curta").Should().Contain("Jwt:Key");
        FalhaAoSubir("Production", "Jwt:Key", "dev-only-key-change-in-production-32ch").Should().Contain("Jwt:Key");
    }

    [Fact]
    public void Host_nao_testing_rejeita_sqlite_em_memoria()
    {
        FalhaAoSubir("Development", "ConnectionStrings:Default", "Data Source=:memory:").Should().Contain("InMemory");
    }

    private static string FalhaAoSubir(string environment, string key, string value)
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseSetting(key, value);
            if (environment == "Production")
            {
                builder.UseSetting("ConnectionStrings:Default", "Data Source=prod-guard.db");
            }
        });

        var act = () => factory.CreateClient();
        var error = act.Should().Throw<Exception>().Which;
        return Flatten(error);
    }

    private static string Flatten(Exception exception)
    {
        var parts = new List<string>();
        for (var current = exception; current is not null; current = current.InnerException)
        {
            parts.Add(current.Message);
        }

        return string.Join(" | ", parts);
    }
}
