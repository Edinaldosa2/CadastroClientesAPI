using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using CadastroCliente.Aplicacao.Abstractions;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Query;
using FluentAssertions;

namespace CadastroCliente.Testes.Integration;

public class AdminApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _admin;
    private readonly HttpClient _editor;
    private readonly HttpClient _leitor;
    private readonly HttpClient _anonimo;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public AdminApiTests(ApiFactory factory)
    {
        _admin = factory.CreateAdminClient();
        _editor = factory.CreateInitializedClient();
        _leitor = factory.CreateLeitorClient();
        _anonimo = factory.CreateAnonymousClient();
    }

    [Fact]
    public async Task Login_admin_emite_jwt_com_tres_papeis()
    {
        var response = await _anonimo.PostAsJsonAsync("/api/v1/auth/token", new LoginRequest
        {
            Usuario = "admin",
            Senha = "admin-dev"
        }, Json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(Json);
        token.Should().NotBeNull();
        token!.Perfil.Should().Be(Perfis.Admin);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        var roles = jwt.Claims
            .Where(c => c.Type is ClaimTypes.Role or "role" or "roles")
            .Select(c => c.Value)
            .ToArray();
        roles.Should().Contain(new[] { Perfis.Admin, Perfis.Escrita, Perfis.Leitura });
    }

    [Fact]
    public async Task Admin_escreve_lista_usuarios_e_auditoria_global()
    {
        var criar = await _admin.PostAsJsonAsync("/api/v1/clientes", new CriarClienteRequest
        {
            Nome = "Cliente Admin",
            TipoPessoa = TipoPessoa.Fisica,
            Documento = "39053344705"
        }, Json);
        criar.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await criar.Content.ReadFromJsonAsync<ClienteDto>(Json);
        criado.Should().NotBeNull();

        (await _admin.GetAsync("/api/v1/clientes")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _admin.GetAsync("/api/v1/clientes?incluirExcluidos=true")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _admin.GetAsync("/api/v1/relatorios/clientes/exportar")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _admin.GetAsync($"/api/v1/clientes/{criado!.Id}/auditoria")).StatusCode.Should().Be(HttpStatusCode.OK);

        var usuariosResp = await _admin.GetAsync("/api/v1/admin/usuarios");
        usuariosResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var raw = await usuariosResp.Content.ReadAsStringAsync();
        raw.Should().NotContain("senha");
        raw.Should().NotContain("admin-dev");
        raw.Should().NotContain("editor-dev");
        var usuarios = await usuariosResp.Content.ReadFromJsonAsync<List<UsuarioAdminDto>>(Json);
        usuarios.Should().NotBeNull();
        usuarios.Should().Contain(u => u.Usuario == "admin" && u.Perfil == Perfis.Admin);
        usuarios.Should().Contain(u => u.Usuario == "editor" && u.Perfil == Perfis.Escrita);
        usuarios.Should().Contain(u => u.Usuario == "leitor" && u.Perfil == Perfis.Leitura);
        usuarios!.Select(u => u.Usuario).Should().OnlyHaveUniqueItems();

        var auditoria = await _admin.GetFromJsonAsync<PagedResult<AuditoriaDto>>("/api/v1/admin/auditoria?pagina=1&tamanhoPagina=20", Json);
        auditoria.Should().NotBeNull();
        auditoria!.Itens.Should().NotBeEmpty();
        auditoria.Itens.Should().Contain(x => x.Usuario == "admin" && x.Tipo == "cliente.criado");
        auditoria.Pagina.Should().Be(1);
        auditoria.TamanhoPagina.Should().Be(20);
    }

    [Fact]
    public async Task Editor_e_leitor_nao_acessam_admin_e_leitor_nao_escreve()
    {
        (await _editor.GetAsync("/api/v1/admin/usuarios")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _editor.GetAsync("/api/v1/admin/auditoria")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _leitor.GetAsync("/api/v1/admin/usuarios")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _leitor.GetAsync("/api/v1/admin/auditoria")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _leitor.PostAsJsonAsync("/api/v1/clientes", new CriarClienteRequest
        {
            Nome = "Leitor Bloqueado",
            TipoPessoa = TipoPessoa.Fisica,
            Documento = "25326781867"
        }, Json)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _editor.GetAsync("/api/v1/clientes")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _editor.GetAsync("/api/v1/relatorios/clientes/exportar")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Anonimo_recebe_401_nas_rotas_admin()
    {
        (await _anonimo.GetAsync("/api/v1/admin/usuarios")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _anonimo.GetAsync("/api/v1/admin/auditoria")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _anonimo.PostAsync("/api/v1/dev/reset", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_pode_resetar_demonstracao()
    {
        var reset = await _admin.PostAsync("/api/v1/dev/reset", null);
        reset.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Catalogo_expoe_rotas_admin()
    {
        var catalogo = await _anonimo.GetFromJsonAsync<JsonElement>("/api");
        catalogo.GetProperty("recursos").GetProperty("usuariosAdmin").GetString().Should().Be("/api/v1/admin/usuarios");
        catalogo.GetProperty("recursos").GetProperty("auditoriaGlobal").GetString().Should().Be("/api/v1/admin/auditoria");
        catalogo.GetProperty("autenticacao").GetProperty("perfis").EnumerateArray()
            .Select(x => x.GetString())
            .Should().Contain(new[] { Perfis.Admin, Perfis.Escrita, Perfis.Leitura });
    }
}
