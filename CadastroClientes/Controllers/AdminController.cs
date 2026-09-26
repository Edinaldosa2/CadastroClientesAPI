using Asp.Versioning;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.Domain.Query;
using CadastroClientes.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Controllers;

[ApiController]
[Authorize(Policy = AppPolicies.Admin)]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class AdminController : ControllerBase
{
    private readonly TokenService _tokens;
    private readonly IAuditoriaAppService _auditoria;

    public AdminController(TokenService tokens, IAuditoriaAppService auditoria)
    {
        _tokens = tokens;
        _auditoria = auditoria;
    }

    /// <summary>Lista os usuários configurados (usuário e perfil, sem senhas).</summary>
    [HttpGet("usuarios")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioAdminDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<UsuarioAdminDto>> Usuarios()
        => Ok(_tokens.ListarUsuarios());

    /// <summary>Lista a trilha de auditoria global com paginação.</summary>
    [HttpGet("auditoria")]
    [ProducesResponseType(typeof(PagedResult<AuditoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuditoriaDto>>> Auditoria(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _auditoria.ListarGlobalAsync(pagina, tamanhoPagina, cancellationToken);
        Response.Headers["X-Total-Count"] = resultado.Total.ToString();
        Response.Headers["X-Page"] = resultado.Pagina.ToString();
        Response.Headers["X-Page-Size"] = resultado.TamanhoPagina.ToString();
        return Ok(resultado);
    }
}
