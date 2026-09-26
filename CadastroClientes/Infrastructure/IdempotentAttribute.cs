using System.Text.Json;
using CadastroCliente.Data.Context;
using CadastroCliente.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace CadastroClientes.Infrastructure;

public sealed class IdempotentAttribute : Attribute, IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues)
            || string.IsNullOrWhiteSpace(keyValues))
        {
            await next();
            return;
        }

        var key = keyValues.ToString().Trim();
        if (key.Length > 80)
        {
            context.Result = new BadRequestObjectResult(new { title = "Idempotency-Key deve ter no máximo 80 caracteres.", code = "idempotency_invalida" });
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<CadastroClienteContext>();
        context.HttpContext.Request.EnableBuffering();
        using var reader = new StreamReader(context.HttpContext.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.HttpContext.Request.Body.Position = 0;
        var hash = IdempotencyStore.HashBody(body);

        var existing = await db.IdempotencyKeys.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key);
        if (existing is not null)
        {
            if (!string.Equals(existing.RequestHash, hash, StringComparison.Ordinal))
            {
                context.Result = new ConflictObjectResult(new { title = "Idempotency-Key já foi usado com outro payload.", code = "idempotency_conflito" });
                return;
            }

            context.HttpContext.Response.Headers["Idempotent-Replayed"] = "true";
            context.Result = new ContentResult
            {
                StatusCode = existing.StatusCode,
                Content = existing.Body,
                ContentType = existing.ContentType
            };
            return;
        }

        var executed = await next();
        if (executed.Result is ObjectResult objectResult && objectResult.StatusCode is >= 200 and < 300)
        {
            var payload = JsonSerializer.Serialize(objectResult.Value);
            db.IdempotencyKeys.Add(new IdempotencyRecord
            {
                Key = key,
                RequestHash = hash,
                StatusCode = objectResult.StatusCode ?? StatusCodes.Status200OK,
                ContentType = "application/json",
                Body = payload,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }
}
