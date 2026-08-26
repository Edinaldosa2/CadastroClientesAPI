using CadastroCliente.Domain.ValueObjects;

namespace CadastroCliente.CrossCutting.Helper;

public static class CepHelper
{
    public static string SomenteDigitos(string? valor)
        => string.IsNullOrWhiteSpace(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());

    public static bool EhValido(string? cep)
    {
        try
        {
            Cep.Criar(cep);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string Formatado(string? cep)
    {
        var digits = SomenteDigitos(cep);
        return digits.Length == 8 ? $"{digits[..5]}-{digits[5..]}" : digits;
    }
}
