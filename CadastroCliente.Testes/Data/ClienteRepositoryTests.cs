using CadastroCliente.Data.Context;
using CadastroCliente.Data.Implementation;
using CadastroCliente.Data.Seed;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Query;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CadastroCliente.Testes.Data;

public class ClienteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CadastroClienteContext _context;
    private readonly ClienteRepository _repository;

    public ClienteRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<CadastroClienteContext>().UseSqlite(_connection).Options;
        _context = new CadastroClienteContext(options);
        _context.Database.EnsureCreated();
        _repository = new ClienteRepository(_context);
    }

    [Fact]
    public async Task Seed_search_estatisticas_e_documento()
    {
        await ClienteSeed.EnsureSeedAsync(_context);
        await ClienteSeed.EnsureSeedAsync(_context);

        var pagina = await _repository.SearchAsync(new ClienteFiltro { Nome = "maria", OrdenarPor = "documento" });
        pagina.Total.Should().Be(1);
        var porDoc = await _repository.GetByDocumentoAsync("529.982.247-25");
        porDoc.Should().NotBeNull();
        (await _repository.ExistsDocumentoAsync("11222333000181")).Should().BeTrue();
        var stats = await _repository.GetEstatisticasAsync();
        stats.Total.Should().Be(2);
        stats.PessoasFisicas.Should().Be(1);

        var sp = await _repository.SearchAsync(new ClienteFiltro { Uf = "SP", Cidade = "São", OrdenarPor = "status", Descendente = true });
        sp.Total.Should().Be(2);

        var cliente = await _repository.GetByIdAsync(porDoc!.Id);
        cliente!.Excluir(DateTimeOffset.UtcNow);
        await _context.SaveChangesAsync();
        (await _repository.GetByIdAsync(cliente.Id)).Should().BeNull();
        (await _repository.GetByIdAsync(cliente.Id, incluirExcluidos: true)).Should().NotBeNull();
        var comExcluidos = await _repository.SearchAsync(new ClienteFiltro { IncluirExcluidos = true, OrdenarPor = "criadoEm" });
        comExcluidos.Total.Should().Be(2);
    }

    [Fact]
    public async Task Add_e_remove()
    {
        var cliente = Cliente.Criar("Novo Cliente", TipoPessoa.Fisica, "11144477735", null, null, null, null, DateTimeOffset.UtcNow);
        await _repository.AddAsync(cliente);
        await _context.SaveChangesAsync();
        var remover = () => _repository.Remove(cliente);
        remover.Should().Throw<InvalidOperationException>().WithMessage("*exclusão lógica*");
        (await _repository.GetByIdAsync(cliente.Id)).Should().NotBeNull();
        (await _repository.ExistsDocumentoAtivoAsync(cliente.Documento)).Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
