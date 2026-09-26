using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Domain.Enums;
using FluentAssertions;

namespace CadastroCliente.Testes.Integration;

public class SecurityGuardsApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _editor;
    private readonly HttpClient _leitor;
    private readonly HttpClient _anonimo;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public SecurityGuardsApiTests(ApiFactory factory)
    {
        _editor = factory.CreateInitializedClient();
        _leitor = factory.CreateLeitorClient();
        _anonimo = factory.CreateAnonymousClient();
    }

    [Fact]
    public async Task Anonimo_nao_le_dados_sensiveis()
    {
        (await _anonimo.GetAsync("/api/v1/clientes")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _anonimo.GetAsync($"/api/v1/clientes/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _anonimo.GetAsync("/api/v1/clientes/documento/52998224725")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _anonimo.GetAsync("/api/v1/relatorios/clientes/resumo")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _anonimo.GetAsync("/api/v1/relatorios/clientes/exportar")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _anonimo.PostAsJsonAsync("/api/v1/clientes", Cliente("25326781867"), Json)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Catalogo_health_e_login_permanecem_publicos()
    {
        (await _anonimo.GetAsync("/api")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _anonimo.GetAsync("/health")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _anonimo.PostAsJsonAsync("/api/v1/auth/token", new LoginRequest { Usuario = "errado", Senha = "x" }, Json))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Leitura_nao_escreve_nem_exporta_nem_lista_excluidos()
    {
        var criado = await Criar(_editor, "71428713859");
        (await _leitor.GetAsync("/api/v1/clientes")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _leitor.PostAsJsonAsync("/api/v1/clientes", Cliente("60839651791"), Json)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _leitor.PutAsJsonAsync($"/api/v1/clientes/{criado.Id}", new AtualizarClienteRequest { Nome = "Hack" }, Json))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _leitor.DeleteAsync($"/api/v1/clientes/{criado.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _leitor.GetAsync("/api/v1/relatorios/clientes/exportar")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _leitor.GetAsync("/api/v1/clientes?incluirExcluidos=true")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _leitor.PostAsync("/api/v1/dev/reset", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_de_colecao_e_id_vazio_sao_bloqueados()
    {
        (await _editor.DeleteAsync("/api/v1/clientes")).StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        var empty = await _editor.GetAsync("/api/v1/clientes/00000000-0000-0000-0000-000000000000");
        empty.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await empty.Content.ReadAsStringAsync();
        body.Should().Contain("id_invalido");
    }

    [Fact]
    public async Task Documento_zero_id_cliente_e_documento_imutavel()
    {
        (await _editor.PostAsJsonAsync("/api/v1/clientes", Cliente("00000000000"), Json)).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var comId = Cliente("10916233090");
        comId.Id = Guid.NewGuid();
        (await _editor.PostAsJsonAsync("/api/v1/clientes", comId, Json)).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var criado = await Criar(_editor, "86288366757");
        var patch = await _editor.PatchAsJsonAsync($"/api/v1/clientes/{criado.Id}", new { documento = "39053344705" }, Json);
        patch.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await patch.Content.ReadAsStringAsync()).Should().Contain("documento_imutavel");
    }

    [Fact]
    public async Task Cliente_bloqueado_nao_edita_e_desbloqueio_exige_motivo()
    {
        var criado = await Criar(_editor, "15350951645");
        (await _editor.PostAsJsonAsync($"/api/v1/clientes/{criado.Id}/bloquear", new AlterarStatusRequest { Motivo = "risco" }, Json))
            .EnsureSuccessStatusCode();

        (await _editor.PutAsJsonAsync($"/api/v1/clientes/{criado.Id}", new AtualizarClienteRequest { Nome = "Novo Nome" }, Json))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var ativar = await _editor.PostAsync($"/api/v1/clientes/{criado.Id}/ativar", null);
        ativar.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ativar.Content.ReadAsStringAsync()).Should().Contain("motivo_obrigatorio");
    }

    [Fact]
    public async Task Etag_conflito_e_idempotencia_com_payload_diferente()
    {
        var criado = await Criar(_editor, "00000010154");
        var get = await _editor.GetAsync($"/api/v1/clientes/{criado.Id}");
        var etag = get.Headers.ETag?.Tag;
        etag.Should().NotBeNullOrWhiteSpace();

        using var stale = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/clientes/{criado.Id}")
        {
            Content = JsonContent.Create(new AtualizarClienteRequest { Nome = "Nome Novo" }, options: Json)
        };
        stale.Headers.TryAddWithoutValidation("If-Match", "\"etag-falso\"");
        var conflito = await _editor.SendAsync(stale);
        conflito.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        (await conflito.Content.ReadAsStringAsync()).Should().Contain("etag_conflito");

        var payload = Cliente("00000010073");
        using var first = new HttpRequestMessage(HttpMethod.Post, "/api/v1/clientes")
        {
            Content = JsonContent.Create(payload, options: Json)
        };
        first.Headers.Add("Idempotency-Key", "guard-key-1");
        (await _editor.SendAsync(first)).StatusCode.Should().Be(HttpStatusCode.Created);

        payload.Nome = "Outro Nome Completo";
        using var second = new HttpRequestMessage(HttpMethod.Post, "/api/v1/clientes")
        {
            Content = JsonContent.Create(payload, options: Json)
        };
        second.Headers.Add("Idempotency-Key", "guard-key-1");
        var replay = await _editor.SendAsync(second);
        replay.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await replay.Content.ReadAsStringAsync()).Should().Contain("idempotency_conflito");
    }

    [Fact]
    public async Task Documento_excluido_continua_unico()
    {
        var original = await Criar(_editor, "00000010235");
        (await _editor.DeleteAsync($"/api/v1/clientes/{original.Id}")).EnsureSuccessStatusCode();

        var substituto = Cliente("00000010235");
        substituto.Nome = "Outro Titular";
        var dup = await _editor.PostAsJsonAsync("/api/v1/clientes", substituto, Json);
        dup.StatusCode.Should().Be(HttpStatusCode.Conflict);

        (await _editor.PostAsync($"/api/v1/clientes/{original.Id}/restaurar", null)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Reset_de_demonstracao_e_exclusivo_do_admin()
    {
        (await _editor.PostAsync("/api/v1/dev/reset", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _leitor.PostAsync("/api/v1/dev/reset", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Csv_export_traz_bom_e_ordenacao_invalida_cai_no_whitelist()
    {
        await Criar(_editor, "00000010316");
        var csv = await _editor.GetAsync("/api/v1/relatorios/clientes/exportar");
        csv.EnsureSuccessStatusCode();
        var bytes = await csv.Content.ReadAsByteArrayAsync();
        bytes.Should().HaveCountGreaterThan(3);
        bytes[0].Should().Be(0xEF);
        bytes[1].Should().Be(0xBB);
        bytes[2].Should().Be(0xBF);

        var lista = await _editor.GetAsync("/api/v1/clientes?ordenarPor=drop-table;--");
        lista.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cursor_contato_e_datas_aceitam_consulta()
    {
        var criado = await Criar(_editor, "00000010405");
        var lista = await _editor.GetAsync($"/api/v1/clientes?contato=@example.com&depoisDe={criado.Id}&criadoDe=2000-01-01&criadoAte=2099-01-01");
        lista.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static CriarClienteRequest Cliente(string documento) => new()
    {
        Nome = "Cliente Guard",
        TipoPessoa = TipoPessoa.Fisica,
        Documento = documento
    };

    private async Task<ClienteDto> Criar(HttpClient client, string documento)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/clientes", Cliente(documento), Json);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<ClienteDto>(Json))!;
    }
}
