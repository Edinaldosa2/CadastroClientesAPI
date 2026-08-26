namespace CadastroCliente.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string resource, object key)
        : base("not_found", $"{resource} '{key}' não foi encontrado.")
    {
    }
}
