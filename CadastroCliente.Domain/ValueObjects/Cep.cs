using CadastroCliente.Domain.Exceptions;

namespace CadastroCliente.Domain.ValueObjects;

public sealed class Cep : IEquatable<Cep>
{
    public string Numero { get; }

    private Cep(string numero)
    {
        Numero = numero;
    }

    public static Cep Criar(string? valor)
    {
        var digits = string.IsNullOrWhiteSpace(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());

        if (digits.Length != 8)
        {
            throw new BusinessRuleException("cep", "CEP deve conter 8 dígitos.");
        }

        if (digits.Distinct().Count() == 1)
        {
            throw new BusinessRuleException("cep", "CEP inválido.");
        }

        return new Cep(digits);
    }

    public string Formatado() => $"{Numero[..5]}-{Numero[5..]}";

    public bool Equals(Cep? other) => other is not null && Numero == other.Numero;

    public override bool Equals(object? obj) => Equals(obj as Cep);

    public override int GetHashCode() => Numero.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Formatado();
}
