using System.Security.Claims;
using CadastroCliente.Aplicacao.Abstractions;

namespace CadastroClientes.Security;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public HttpCurrentUser(IHttpContextAccessor http)
    {
        _http = http;
    }

    public bool Autenticado => _http.HttpContext?.User.Identity?.IsAuthenticated == true;

    public string? Usuario
    {
        get
        {
            var user = _http.HttpContext?.User;
            if (user is null)
            {
                return null;
            }

            return user.FindFirstValue(ClaimTypes.Name)
                   ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? user.Identity?.Name;
        }
    }

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var user = _http.HttpContext?.User;
            if (user is null)
            {
                return Array.Empty<string>();
            }

            return user.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public bool IsAdmin => Possui(Perfis.Admin);

    public bool PodeEscrever => IsAdmin || Possui(Perfis.Escrita);

    public bool PodeLer => PodeEscrever || Possui(Perfis.Leitura);

    private bool Possui(string perfil) => _http.HttpContext?.User.IsInRole(perfil) == true;
}
