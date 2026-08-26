using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Domain.Enums;
using FluentAssertions;

namespace CadastroCliente.Testes.Integration;

public class NestedResourcesApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public NestedResourcesApiTests(ApiFactory factory)
    {
        _client = factory.CreateInitializedClient();
    }

    [Fact]
    public async Task Enderecos_e_contatos()
    {
        var criar = await _client.PostAsJsonAsync("/api/v1/clientes", new CriarClienteRequest
        {
            Nome = "Empresa Exemplo Ltda",
            NomeFantasia = "Exemplo",
            TipoPessoa = TipoPessoa.Juridica,
            Documento = "11222333000181"
        }, Json);
        criar.EnsureSuccessStatusCode();
        var cliente = await criar.Content.ReadFromJsonAsync<ClienteDto>(Json);

        var enderecoResp = await _client.PostAsJsonAsync($"/api/v1/clientes/{cliente!.Id}/enderecos", new EnderecoRequest
        {
            Tipo = TipoEndereco.Comercial,
            Logradouro = "Avenida Brasil",
            Numero = "1500",
            Bairro = "Centro",
            Cidade = "Rio de Janeiro",
            Uf = "RJ",
            Cep = "20040002",
            Principal = true
        }, Json);
        enderecoResp.StatusCode.Should().Be(HttpStatusCode.Created, await enderecoResp.Content.ReadAsStringAsync());
        var endereco = await enderecoResp.Content.ReadFromJsonAsync<EnderecoDto>(Json);

        (await _client.GetAsync($"/api/v1/clientes/{cliente.Id}/enderecos")).EnsureSuccessStatusCode();
        (await _client.GetAsync($"/api/v1/clientes/{cliente.Id}/enderecos/{endereco!.Id}")).EnsureSuccessStatusCode();

        var segundo = await (await _client.PostAsJsonAsync($"/api/v1/clientes/{cliente.Id}/enderecos", new EnderecoRequest
        {
            Tipo = TipoEndereco.Entrega,
            Logradouro = "Rua do Ouvidor",
            Numero = "50",
            Bairro = "Centro",
            Cidade = "Rio de Janeiro",
            Uf = "RJ",
            Cep = "20040030"
        }, Json)).Content.ReadFromJsonAsync<EnderecoDto>(Json);

        (await _client.PatchAsync($"/api/v1/clientes/{cliente.Id}/enderecos/{segundo!.Id}/principal", null)).EnsureSuccessStatusCode();
        (await _client.PutAsJsonAsync($"/api/v1/clientes/{cliente.Id}/enderecos/{endereco.Id}", new EnderecoRequest
        {
            Tipo = TipoEndereco.Cobranca,
            Logradouro = "Avenida Brasil",
            Numero = "1500",
            Complemento = "Sala 2",
            Bairro = "Centro",
            Cidade = "Rio de Janeiro",
            Uf = "RJ",
            Cep = "20040002"
        }, Json)).EnsureSuccessStatusCode();
        (await _client.DeleteAsync($"/api/v1/clientes/{cliente.Id}/enderecos/{segundo.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var contatoResp = await _client.PostAsJsonAsync($"/api/v1/clientes/{cliente.Id}/contatos", new ContatoRequest
        {
            Tipo = TipoContato.Telefone,
            Valor = "2133334444",
            Nome = "Recepção"
        }, Json);
        contatoResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var contato = await contatoResp.Content.ReadFromJsonAsync<ContatoDto>(Json);
        (await _client.GetAsync($"/api/v1/clientes/{cliente.Id}/contatos")).EnsureSuccessStatusCode();
        (await _client.PutAsJsonAsync($"/api/v1/clientes/{cliente.Id}/contatos/{contato!.Id}", new ContatoRequest
        {
            Tipo = TipoContato.WhatsApp,
            Valor = "21988887777",
            Nome = "Comercial"
        }, Json)).EnsureSuccessStatusCode();
        (await _client.DeleteAsync($"/api/v1/clientes/{cliente.Id}/contatos/{contato.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
