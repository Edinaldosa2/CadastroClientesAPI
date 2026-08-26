namespace CadastroCliente.Domain.Entities;

public abstract class EntityBase
{
    public Guid Id { get; protected set; }
    public DateTime CriadoEm { get; protected set; }
    public DateTime? AtualizadoEm { get; protected set; }

    protected EntityBase()
    {
        Id = Guid.NewGuid();
        CriadoEm = DateTime.UtcNow;
    }

    protected void Tocar(DateTimeOffset agora)
    {
        AtualizadoEm = agora.UtcDateTime;
    }
}
