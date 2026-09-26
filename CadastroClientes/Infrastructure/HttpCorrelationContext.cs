using CadastroCliente.Aplicacao.Abstractions;

namespace CadastroClientes.Infrastructure;

public sealed class HttpCorrelationContext : ICorrelationContext
{
    private readonly IHttpContextAccessor _http;

    public HttpCorrelationContext(IHttpContextAccessor http)
    {
        _http = http;
    }

    public string? CorrelationId
    {
        get
        {
            var context = _http.HttpContext;
            if (context is null)
            {
                return null;
            }

            if (context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var stored)
                && stored is string value
                && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return context.TraceIdentifier;
        }
    }
}
