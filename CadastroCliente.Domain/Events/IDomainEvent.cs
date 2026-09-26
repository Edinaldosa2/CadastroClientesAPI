namespace CadastroCliente.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OcorridoEm { get; }
}
