namespace CadastroCliente.Domain.Exceptions;

public sealed class BusinessRuleException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public BusinessRuleException(string message)
        : this("business_rule", message, new Dictionary<string, string[]>())
    {
    }

    public BusinessRuleException(string field, string message)
        : this("validation", message, new Dictionary<string, string[]> { [field] = new[] { message } })
    {
    }

    public BusinessRuleException(IReadOnlyDictionary<string, string[]> errors)
        : this("validation", "Um ou mais campos são inválidos.", errors)
    {
    }

    private BusinessRuleException(string code, string message, IReadOnlyDictionary<string, string[]> errors)
        : base(code, message)
    {
        Errors = errors;
    }
}
