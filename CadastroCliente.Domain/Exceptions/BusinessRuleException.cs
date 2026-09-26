namespace CadastroCliente.Domain.Exceptions;

public sealed class BusinessRuleException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public BusinessRuleException(string message)
        : this("business_rule", message, new Dictionary<string, string[]>())
    {
    }

    public BusinessRuleException(string field, string message)
        : this(CodigoDe(field, message), message, new Dictionary<string, string[]> { [field] = new[] { message } })
    {
    }

    public BusinessRuleException(string code, string field, string message)
        : this(code, message, new Dictionary<string, string[]> { [field] = new[] { message } })
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

    private static string CodigoDe(string field, string message)
    {
        var texto = message.ToLowerInvariant();
        if (texto.Contains("cpf")) return "cpf_invalido";
        if (texto.Contains("cnpj")) return "cnpj_invalido";
        if (field == "cep") return "cep_invalido";
        if (field == "uf") return "uf_invalida";
        if (field == "status" && texto.Contains("bloqueado")) return "cliente_bloqueado";
        if (field == "motivo") return "motivo_obrigatorio";
        if (field == "id") return "id_invalido";
        if (field == "documento" && texto.Contains("não pode")) return "documento_imutavel";
        return field switch
        {
            "documento" => "documento_invalido",
            "enderecos" => "limite_enderecos",
            "contatos" => "limite_contatos",
            _ => "business_rule"
        };
    }
}
