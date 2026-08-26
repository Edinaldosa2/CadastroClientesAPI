using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Exceptions;

namespace CadastroCliente.Domain.ValueObjects;

public sealed class Documento : IEquatable<Documento>
{
    public string Numero { get; }
    public TipoPessoa Tipo { get; }

    private Documento(string numero, TipoPessoa tipo)
    {
        Numero = numero;
        Tipo = tipo;
    }

    public static Documento Criar(string? valor, TipoPessoa tipo)
    {
        var digits = SomenteDigitos(valor);
        if (tipo == TipoPessoa.Fisica)
        {
            if (!EhCpfValido(digits))
            {
                throw new BusinessRuleException("documento", "CPF inválido.");
            }

            return new Documento(digits, tipo);
        }

        if (!EhCnpjValido(digits))
        {
            throw new BusinessRuleException("documento", "CNPJ inválido.");
        }

        return new Documento(digits, tipo);
    }

    public string Formatado()
    {
        if (Tipo == TipoPessoa.Fisica)
        {
            return $"{Numero[..3]}.{Numero[3..6]}.{Numero[6..9]}-{Numero[9..]}";
        }

        return $"{Numero[..2]}.{Numero[2..5]}.{Numero[5..8]}/{Numero[8..12]}-{Numero[12..]}";
    }

    public static string SomenteDigitos(string? valor)
        => string.IsNullOrWhiteSpace(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());

    public static bool EhCpfValido(string? cpf)
    {
        var digits = SomenteDigitos(cpf);
        if (digits.Length != 11)
        {
            return false;
        }

        if (digits.Distinct().Count() == 1)
        {
            return false;
        }

        var d1 = CalcularDigito(digits, 9, 10);
        if (digits[9] - '0' != d1)
        {
            return false;
        }

        var d2 = CalcularDigito(digits, 10, 11);
        return digits[10] - '0' == d2;
    }

    public static bool EhCnpjValido(string? cnpj)
    {
        var digits = SomenteDigitos(cnpj);
        if (digits.Length != 14)
        {
            return false;
        }

        if (digits.Distinct().Count() == 1)
        {
            return false;
        }

        int[] w1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] w2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var d1 = CalcularDigitoComPesos(digits, w1);
        if (digits[12] - '0' != d1)
        {
            return false;
        }

        var d2 = CalcularDigitoComPesos(digits, w2);
        return digits[13] - '0' == d2;
    }

    private static int CalcularDigito(string digits, int length, int pesoInicial)
    {
        var soma = 0;
        for (var i = 0; i < length; i++)
        {
            soma += (digits[i] - '0') * (pesoInicial - i);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int CalcularDigitoComPesos(string digits, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < pesos.Length; i++)
        {
            soma += (digits[i] - '0') * pesos[i];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public bool Equals(Documento? other)
        => other is not null && Numero == other.Numero && Tipo == other.Tipo;

    public override bool Equals(object? obj) => Equals(obj as Documento);

    public override int GetHashCode() => HashCode.Combine(Numero, Tipo);

    public override string ToString() => Formatado();
}
