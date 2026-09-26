using System.Net;
using CadastroCliente.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroClientes.Infrastructure;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, title, errors) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
            ConflictException => (HttpStatusCode.Conflict, exception.Message, null),
            PreconditionFailedException => (HttpStatusCode.PreconditionFailed, exception.Message, null),
            BusinessRuleException business => (HttpStatusCode.UnprocessableEntity, business.Message, business.Errors),
            ValidationException validation => (HttpStatusCode.BadRequest, "Um ou mais campos são inválidos.",
                validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => string.IsNullOrWhiteSpace(g.Key) ? "request" : ToCamel(g.Key),
                        g => g.Select(e => e.ErrorMessage).ToArray())),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "O registro foi alterado por outro processo. Recarregue e tente novamente.", null),
            _ => (HttpStatusCode.InternalServerError, "Erro interno ao processar a requisição.", (IReadOnlyDictionary<string, string[]>?)null)
        };

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Type = $"https://httpstatuses.com/{(int)status}",
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (exception is DomainException domain)
        {
            problem.Extensions["code"] = domain.Code;
        }
        if (errors is { Count: > 0 })
        {
            problem.Extensions["errors"] = errors;
        }

        if (status == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
            var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
            if (!env.IsProduction())
            {
                problem.Detail = exception.ToString();
            }
        }
        else
        {
            _logger.LogWarning(exception, "Handled domain exception {Status}", status);
        }

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }

    private static string ToCamel(string value)
    {
        var last = value.Contains('.') ? value.Split('.')[^1] : value;
        return string.IsNullOrEmpty(last) ? last : char.ToLowerInvariant(last[0]) + last[1..];
    }
}
