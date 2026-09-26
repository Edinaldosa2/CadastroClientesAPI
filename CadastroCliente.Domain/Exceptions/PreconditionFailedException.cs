namespace CadastroCliente.Domain.Exceptions;

public sealed class PreconditionFailedException : DomainException
{
    public PreconditionFailedException(string message)
        : base("etag_conflito", message)
    {
    }
}
