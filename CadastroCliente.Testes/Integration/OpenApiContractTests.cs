using System.Text.Json;
using FluentAssertions;

namespace CadastroCliente.Testes.Integration;

public class OpenApiContractTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public OpenApiContractTests(ApiFactory factory)
    {
        _client = factory.CreateAnonymousClient();
    }

    [Fact]
    public async Task Swagger_expoe_rotas_seguranca_e_esquemas()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        var paths = root.GetProperty("paths");

        paths.TryGetProperty("/api/v1/auth/token", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/clientes", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/clientes/{id}", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/clientes/documento/{documento}", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/clientes/{id}/ativar", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/clientes/{id}/restaurar", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/clientes/{id}/auditoria", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/clientes/{clienteId}/enderecos", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/clientes/{clienteId}/contatos", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/relatorios/clientes/resumo", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/relatorios/clientes/exportar", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/dev/reset", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/admin/usuarios", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/admin/auditoria", out _).Should().BeTrue();

        var clientes = paths.GetProperty("/api/v1/clientes");
        clientes.TryGetProperty("get", out _).Should().BeTrue();
        clientes.TryGetProperty("post", out _).Should().BeTrue();
        clientes.TryGetProperty("delete", out _).Should().BeTrue();

        var schemes = root.GetProperty("components").GetProperty("securitySchemes");
        schemes.TryGetProperty("Bearer", out var bearer).Should().BeTrue();
        bearer.GetProperty("scheme").GetString().Should().Be("bearer");
    }
}
