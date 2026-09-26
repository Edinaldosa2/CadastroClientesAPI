using CadastroCliente.Domain.Events;

namespace CadastroCliente.Domain.Entities;

public abstract class EntityBase
{
    private readonly List<IDomainEvent> _eventos = new();

    public Guid Id { get; protected set; }
    public DateTime CriadoEm { get; protected set; }
    public DateTime? AtualizadoEm { get; protected set; }
    public IReadOnlyCollection<IDomainEvent> Eventos => _eventos.AsReadOnly();

    protected EntityBase()
    {
        Id = Guid.NewGuid();
        CriadoEm = DateTime.UtcNow;
    }

    protected void Tocar(DateTimeOffset agora)
    {
        AtualizadoEm = agora.UtcDateTime;
    }

    protected void RegistrarEvento(IDomainEvent evento) => _eventos.Add(evento);

    public IReadOnlyList<IDomainEvent> ConsumirEventos()
    {
        var copia = _eventos.ToArray();
        _eventos.Clear();
        return copia;
    }
}
