using CadastroCliente.Data.Context;
using CadastroCliente.Data.Implementation;
using CadastroCliente.Domain.Query;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CadastroCliente.Testes.Data;

public class AuditoriaRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CadastroClienteContext _context;
    private readonly AuditoriaRepository _repository;

    public AuditoriaRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<CadastroClienteContext>().UseSqlite(_connection).Options;
        _context = new CadastroClienteContext(options);
        _context.Database.EnsureCreated();
        _repository = new AuditoriaRepository(_context);
    }

    [Fact]
    public async Task Listar_global_pagina_e_ordena_recente()
    {
        await _repository.AddAsync(new AuditoriaItem
        {
            ClienteId = Guid.NewGuid(),
            Tipo = "cliente.criado",
            Descricao = "antigo",
            Usuario = "editor",
            OcorridoEm = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        });
        await _repository.AddAsync(new AuditoriaItem
        {
            ClienteId = Guid.NewGuid(),
            Tipo = "cliente.atualizado",
            Descricao = "recente",
            Usuario = "admin",
            OcorridoEm = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        });
        await _context.SaveChangesAsync();

        var pagina = await _repository.ListarAsync(1, 1);
        pagina.Total.Should().Be(2);
        pagina.TamanhoPagina.Should().Be(1);
        pagina.Itens.Should().ContainSingle(x => x.Usuario == "admin" && x.Tipo == "cliente.atualizado");

        var segunda = await _repository.ListarAsync(2, 1);
        segunda.Itens.Should().ContainSingle(x => x.Usuario == "editor");
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
