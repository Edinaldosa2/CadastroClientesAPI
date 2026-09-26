using CadastroCliente.CrossCutting.Helper;

namespace CadastroClientes.Infrastructure;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = context.TraceIdentifier,
            ["Path"] = context.Request.Path.ToString(),
            ["Method"] = context.Request.Method
        }))
        {
            _logger.LogInformation("HTTP {Method} {Path}", context.Request.Method, context.Request.Path);
            await _next(context);
        }
    }

    public static string ClienteLog(Guid id, string? documento)
        => $"clienteId={id} documento={DocumentoHelper.Mascarado(documento)}";
}
