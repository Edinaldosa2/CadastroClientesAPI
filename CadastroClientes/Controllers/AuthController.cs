using Asp.Versioning;
using CadastroCliente.Aplicacao.DTOs;
using CadastroClientes.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Controllers;

[ApiController]
[AllowAnonymous]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly TokenService _tokens;

    public AuthController(TokenService tokens)
    {
        _tokens = tokens;
    }

    /// <summary>Emite um JWT com perfil admin, escrita ou leitura.</summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<TokenResponse> Token([FromBody] LoginRequest request)
    {
        try
        {
            return Ok(_tokens.Emitir(request));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { title = "Credenciais inválidas.", status = 401, code = "credenciais_invalidas" });
        }
    }
}
