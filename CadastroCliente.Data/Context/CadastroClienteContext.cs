using CadastroCliente.Data.Mapping;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CadastroCliente.Data.Context;

public sealed class CadastroClienteContext : DbContext, IUnitOfWork
{
    public CadastroClienteContext(DbContextOptions<CadastroClienteContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Contato> Contatos => Set<Contato>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ClienteMap());
        modelBuilder.ApplyConfiguration(new EnderecoMap());
        modelBuilder.ApplyConfiguration(new ContatoMap());
        base.OnModelCreating(modelBuilder);
    }
}
