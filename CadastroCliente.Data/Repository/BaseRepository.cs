using CadastroCliente.Data.Context;
using CadastroCliente.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadastroCliente.Data.Repository;

public abstract class BaseRepository<TEntity> where TEntity : EntityBase
{
    protected readonly CadastroClienteContext Context;
    protected readonly DbSet<TEntity> Set;

    protected BaseRepository(CadastroClienteContext context)
    {
        Context = context;
        Set = context.Set<TEntity>();
    }
}
