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
}
