namespace CadastroCliente.Domain.Exceptions;

public sealed class ConflictException : DomainException
{
    public ConflictException(string message)
        : base("conflict", message)
    {
    }
}
