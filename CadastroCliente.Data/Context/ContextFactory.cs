using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CadastroCliente.Data.Context;

public sealed class ContextFactory : IDesignTimeDbContextFactory<CadastroClienteContext>
{
    public CadastroClienteContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CadastroClienteContext>()
            .UseSqlite("Data Source=cadastro-clientes.db")
            .Options;

        return new CadastroClienteContext(options);
    }
}
