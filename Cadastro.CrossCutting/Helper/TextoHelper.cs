namespace CadastroCliente.CrossCutting.Helper;

public static class TextoHelper
{
    public static string? NuloSeVazio(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    public static string Normalizar(string? valor)
        => valor?.Trim() ?? string.Empty;
}
