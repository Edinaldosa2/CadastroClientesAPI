using Asp.Versioning;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/relatorios/clientes")]
[Produces("application/json")]
public sealed class RelatoriosController : ControllerBase
{
    private readonly IRelatorioAppService _service;

    public RelatoriosController(IRelatorioAppService service)
    {
        _service = service;
    }

    /// <summary>Resumo quantitativo da base de clientes.</summary>
    [HttpGet("resumo")]
    [ProducesResponseType(typeof(RelatorioResumoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RelatorioResumoDto>> Resumo(CancellationToken cancellationToken)
        => Ok(await _service.ObterResumoAsync(cancellationToken));

    /// <summary>Exporta a listagem de clientes em CSV.</summary>
    [HttpGet("exportar")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Exportar(CancellationToken cancellationToken)
    {
        var csv = await _service.ExportarCsvAsync(cancellationToken);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "clientes.csv");
    }
}
