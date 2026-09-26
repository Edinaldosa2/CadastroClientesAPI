using Asp.Versioning;
using CadastroCliente.Data.Context;
using CadastroCliente.Data.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroClientes.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = "escrita")]
[Route("api/v{version:apiVersion}/dev")]
[Produces("application/json")]
public sealed class DevController : ControllerBase
{
    private readonly CadastroClienteContext _db;
    private readonly IHostEnvironment _environment;

    public DevController(CadastroClienteContext db, IHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    /// <summary>Recria o banco de demonstração. Bloqueado fora de Development/Testing.</summary>
    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        if (_environment.IsProduction())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { title = "Reset bloqueado em produção.", code = "reset_bloqueado" });
        }

        await _db.Database.EnsureCreatedAsync(cancellationToken);
        _db.Contatos.RemoveRange(_db.Contatos);
        _db.Enderecos.RemoveRange(_db.Enderecos);
        _db.Clientes.RemoveRange(_db.Clientes.IgnoreQueryFilters());

        await _db.SaveChangesAsync(cancellationToken);
        await ClienteSeed.EnsureSeedAsync(_db, cancellationToken);
        return Ok(new { title = "Base de demonstração recriada." });
    }
}
