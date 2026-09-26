using CadastroCliente.Aplicacao.DTOs;

namespace CadastroClientes.Security;

public static class EtagHelper
{
    public static string From(ClienteDto cliente)
        => $"\"{cliente.Id:N}-{cliente.AtualizadoEm?.UtcTicks ?? cliente.CriadoEm.UtcTicks}\"";

    public static bool Matches(string? ifMatch, string etag)
    {
        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return true;
        }

        var expected = ifMatch.Trim();
        return string.Equals(expected, etag, StringComparison.Ordinal)
               || string.Equals(expected, etag.Trim('"'), StringComparison.Ordinal);
    }
}
