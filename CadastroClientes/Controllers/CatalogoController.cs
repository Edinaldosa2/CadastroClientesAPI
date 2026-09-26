using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Controllers;

[ApiController]
[AllowAnonymous]
[ApiVersion("1.0")]
[Route("api")]
[Produces("application/json")]
public sealed class CatalogoController : ControllerBase
{
    /// <summary>Catálogo das rotas públicas da API.</summary>
    [HttpGet]
    [HttpGet("v{version:apiVersion}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok(new
        {
            nome = "Cadastro de Clientes API",
            versao = "1.0",
            documentacao = "/swagger",
            autenticacao = "/api/v1/auth/token",
            saude = new { live = "/health/live", ready = "/health/ready", full = "/health" },
            limites = new { enderecosPorCliente = 10, contatosPorCliente = 15, tamanhoPaginaMaximo = 100 },
            recursos = new
            {
                clientes = "/api/v1/clientes",
                clientePorDocumento = "/api/v1/clientes/documento/{documento}",
                auditoria = "/api/v1/clientes/{id}/auditoria",
                enderecos = "/api/v1/clientes/{id}/enderecos",
                contatos = "/api/v1/clientes/{id}/contatos",
                relatorioResumo = "/api/v1/relatorios/clientes/resumo",
                exportacaoCsv = "/api/v1/relatorios/clientes/exportar",
                resetDemonstracao = "/api/v1/dev/reset"
            }
        });
}
