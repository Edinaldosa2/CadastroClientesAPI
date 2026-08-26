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

    public HttpClient CreateInitializedClient()
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CadastroClienteContext>();
        db.Database.EnsureCreated();
        return client;
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
