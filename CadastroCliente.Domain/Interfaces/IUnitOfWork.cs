using CadastroCliente.Domain.Entities;

namespace CadastroCliente.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void RegisterNew<T>(T entity) where T : EntityBase;
}
