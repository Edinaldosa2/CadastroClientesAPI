namespace CadastroCliente.Domain.Entities;

public abstract class EntityBase
{
    public Guid Id { get; protected set; }
    public DateTimeOffset CriadoEm { get; protected set; }
    public DateTimeOffset? AtualizadoEm { get; protected set; }

    protected EntityBase()
    {
        Id = Guid.NewGuid();
        CriadoEm = DateTimeOffset.UtcNow;
    }

    protected void Tocar(DateTimeOffset agora)
    {
        AtualizadoEm = agora;
    }
}
