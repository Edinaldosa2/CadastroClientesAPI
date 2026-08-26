using CadastroCliente.Domain.Interfaces;

namespace CadastroCliente.CrossCutting.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
