using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Controllers;

[ApiController]
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
            saude = new { live = "/health/live", ready = "/health/ready", full = "/health" },
            recursos = new
            {
                clientes = "/api/v1/clientes",
                clientePorDocumento = "/api/v1/clientes/documento/{documento}",
                enderecos = "/api/v1/clientes/{id}/enderecos",
                contatos = "/api/v1/clientes/{id}/contatos",
                relatorioResumo = "/api/v1/relatorios/clientes/resumo",
                exportacaoCsv = "/api/v1/relatorios/clientes/exportar"
            }
        });
}
