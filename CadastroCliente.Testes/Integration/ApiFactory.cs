using System.Net.Http.Headers;
using System.Net.Http.Json;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroCliente.Testes.Integration;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new($"Data Source=file:cadastro-testes-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");

    public ApiFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            var stale = services.Where(d =>
                    d.ServiceType == typeof(CadastroClienteContext) ||
                    d.ServiceType == typeof(DbContextOptions<CadastroClienteContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                .ToList();
            foreach (var descriptor in stale)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<CadastroClienteContext>(options => options.UseSqlite(_connection));
        });
    }

    public HttpClient CreateAnonymousClient()
    {
        var client = CreateClient();
        EnsureCreated();
        return client;
    }

    public HttpClient CreateInitializedClient() => CreateAuthenticatedClient("editor", "editor-dev");

    public HttpClient CreateLeitorClient() => CreateAuthenticatedClient("leitor", "leitor-dev");

    public HttpClient CreateAuthenticatedClient(string usuario, string senha)
    {
        var client = CreateAnonymousClient();
        var token = Login(client, usuario, senha);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static string Login(HttpClient client, string usuario, string senha)
    {
        var response = client.PostAsJsonAsync("/api/v1/auth/token", new LoginRequest
        {
            Usuario = usuario,
            Senha = senha
        }).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var body = response.Content.ReadFromJsonAsync<TokenResponse>().GetAwaiter().GetResult();
        return body?.AccessToken ?? throw new InvalidOperationException("Token não emitido.");
    }

    private void EnsureCreated()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CadastroClienteContext>();
        db.Database.EnsureCreated();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
