using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Query;
using FluentAssertions;

namespace CadastroCliente.Testes.Integration;

public class ClientesApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public ClientesApiTests(ApiFactory factory)
    {
        _client = factory.CreateInitializedClient();
    }

    [Fact]
    public async Task Catalogo_health_e_swagger()
    {
        (await _client.GetAsync("/api")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.GetAsync("/api/v1")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.GetAsync("/health")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.GetAsync("/health/live")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.GetAsync("/health/ready")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.GetAsync("/swagger/v1/swagger.json")).StatusCode.Should().Be(HttpStatusCode.OK);
        var redirect = await _client.GetAsync("/");
        redirect.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Fluxo_completo_de_cliente()
    {
        var criar = new CriarClienteRequest
        {
            Nome = "João da Silva",
            TipoPessoa = TipoPessoa.Fisica,
            Documento = "11144477735",
            DataNascimento = new DateTime(1985, 3, 10),
            Observacoes = "Cliente de teste",
            Enderecos =
            {
                new EnderecoRequest
                {
                    Tipo = TipoEndereco.Residencial,
                    Logradouro = "Rua Augusta",
                    Numero = "200",
                    Bairro = "Consolação",
                    Cidade = "São Paulo",
                    Uf = "SP",
                    Cep = "01305000",
                    Principal = true
                }
            },
            Contatos =
            {
                new ContatoRequest { Tipo = TipoContato.Email, Valor = "joao@example.com", Principal = true }
            }
        };

        var criadoResp = await _client.PostAsJsonAsync("/api/v1/clientes", criar, Json);
        criadoResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await criadoResp.Content.ReadFromJsonAsync<ClienteDto>(Json);
        criado.Should().NotBeNull();
        criado!.Documento.Should().Be("11144477735");

        var list = await _client.GetFromJsonAsync<PagedResult<ClienteListItemDto>>("/api/v1/clientes?nome=joão&uf=SP", Json);
        list!.Total.Should().BeGreaterThan(0);

        var porDoc = await _client.GetAsync($"/api/v1/clientes/documento/{criado.Documento}");
        porDoc.StatusCode.Should().Be(HttpStatusCode.OK);

        var put = await _client.PutAsJsonAsync($"/api/v1/clientes/{criado.Id}", new AtualizarClienteRequest { Nome = "João Silva Souza" }, Json);
        put.EnsureSuccessStatusCode();

        var patch = await _client.PatchAsJsonAsync($"/api/v1/clientes/{criado.Id}", new PatchClienteRequest { Observacoes = "atualizado", AtualizarObservacoes = true }, Json);
        patch.EnsureSuccessStatusCode();

        (await _client.PostAsJsonAsync($"/api/v1/clientes/{criado.Id}/inativar", new AlterarStatusRequest { Motivo = "teste" }, Json)).EnsureSuccessStatusCode();
        (await _client.PostAsync($"/api/v1/clientes/{criado.Id}/ativar", null)).EnsureSuccessStatusCode();
        (await _client.PostAsJsonAsync($"/api/v1/clientes/{criado.Id}/bloquear", new AlterarStatusRequest { Motivo = "fraude" }, Json)).EnsureSuccessStatusCode();
        (await _client.PostAsJsonAsync($"/api/v1/clientes/{criado.Id}/ativar", new AlterarStatusRequest { Motivo = "revisão" }, Json)).EnsureSuccessStatusCode();

        (await _client.DeleteAsync($"/api/v1/clientes/{criado.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.GetAsync($"/api/v1/clientes/{criado.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await _client.PostAsync($"/api/v1/clientes/{criado.Id}/restaurar", null)).EnsureSuccessStatusCode();
        (await _client.GetAsync($"/api/v1/clientes/{criado.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Validacao_e_conflito()
    {
        var invalido = await _client.PostAsJsonAsync("/api/v1/clientes", new CriarClienteRequest { Nome = "Al", TipoPessoa = TipoPessoa.Fisica, Documento = "123" }, Json);
        invalido.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var payload = new CriarClienteRequest { Nome = "Ana Costa", TipoPessoa = TipoPessoa.Fisica, Documento = "52998224725" };
        (await _client.PostAsJsonAsync("/api/v1/clientes", payload, Json)).EnsureSuccessStatusCode();
        var dup = await _client.PostAsJsonAsync("/api/v1/clientes", payload, Json);
        dup.StatusCode.Should().Be(HttpStatusCode.Conflict);

        (await _client.GetAsync($"/api/v1/clientes/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Idempotencia_repete_post()
    {
        var payload = new CriarClienteRequest { Nome = "Carlos Lima", TipoPessoa = TipoPessoa.Fisica, Documento = "39053344705" };
        using var first = new HttpRequestMessage(HttpMethod.Post, "/api/v1/clientes") { Content = JsonContent.Create(payload, options: Json) };
        first.Headers.Add("Idempotency-Key", "key-carlos-1");
        var r1 = await _client.SendAsync(first);
        r1.StatusCode.Should().Be(HttpStatusCode.Created);

        using var second = new HttpRequestMessage(HttpMethod.Post, "/api/v1/clientes") { Content = JsonContent.Create(payload, options: Json) };
        second.Headers.Add("Idempotency-Key", "key-carlos-1");
        var r2 = await _client.SendAsync(second);
        r2.StatusCode.Should().Be(HttpStatusCode.Created);
        r2.Headers.Contains("Idempotent-Replayed").Should().BeTrue();
    }

    [Fact]
    public async Task Relatorios()
    {
        var resumo = await _client.GetAsync("/api/v1/relatorios/clientes/resumo");
        resumo.EnsureSuccessStatusCode();
        var csv = await _client.GetAsync("/api/v1/relatorios/clientes/exportar");
        csv.EnsureSuccessStatusCode();
        csv.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    }
}
