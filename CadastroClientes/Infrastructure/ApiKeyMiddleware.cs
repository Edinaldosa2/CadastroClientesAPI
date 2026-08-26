namespace CadastroClientes.Infrastructure;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var configured = _configuration["Security:ApiKey"];
        if (string.IsNullOrWhiteSpace(configured) || IsAnonymous(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var provided) || provided != configured)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { title = "API key inválida ou ausente.", status = 401 });
            return;
        }

        await _next(context);
    }

    private static bool IsAnonymous(PathString path)
        => path.StartsWithSegments("/health")
           || path.StartsWithSegments("/swagger")
           || path.StartsWithSegments("/favicon")
           || path == "/";
}
