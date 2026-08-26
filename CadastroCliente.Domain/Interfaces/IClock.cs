namespace CadastroCliente.Domain.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
