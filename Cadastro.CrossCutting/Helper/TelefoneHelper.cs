using System.Text.RegularExpressions;

namespace CadastroCliente.CrossCutting.Helper;

public static class TelefoneHelper
{
    private static readonly Regex Digits = new(@"\D", RegexOptions.Compiled);

    public static string SomenteDigitos(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? string.Empty : Digits.Replace(valor, string.Empty);

    public static bool EhValido(string? telefone)
    {
        var digits = SomenteDigitos(telefone);
        return digits.Length is >= 10 and <= 13;
    }

    public static string Formatado(string? telefone)
    {
        var digits = SomenteDigitos(telefone);
        return digits.Length switch
        {
            10 => $"({digits[..2]}) {digits[2..6]}-{digits[6..]}",
            11 => $"({digits[..2]}) {digits[2..7]}-{digits[7..]}",
            _ => digits
        };
    }
}
