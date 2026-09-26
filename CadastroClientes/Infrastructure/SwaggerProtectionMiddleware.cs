namespace CadastroClientes.Infrastructure;

public sealed class SwaggerProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;

    public SwaggerProtectionMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_environment.IsProduction()
            && context.Request.Path.StartsWithSegments("/swagger")
            && context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { title = "Swagger exige autenticação em produção.", status = 401 });
            return;
        }

        await _next(context);
    }
}
