using Asp.Versioning;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Controllers;

[ApiController]
[Authorize(Roles = "leitura,escrita")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes/{clienteId:guid}/contatos")]
[Produces("application/json")]
public sealed class ContatosController : ControllerBase
{
    private readonly IContatoAppService _service;

    public ContatosController(IContatoAppService service)
    {
        _service = service;
    }

    /// <summary>Lista os contatos do cliente.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContatoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContatoDto>>> Listar(Guid clienteId, CancellationToken cancellationToken)
        => Ok(await _service.ListarAsync(clienteId, cancellationToken));

    /// <summary>Obtém um contato específico.</summary>
    [HttpGet("{contatoId:guid}")]
    [ProducesResponseType(typeof(ContatoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContatoDto>> Obter(Guid clienteId, Guid contatoId, CancellationToken cancellationToken)
        => Ok(await _service.ObterAsync(clienteId, contatoId, cancellationToken));

    /// <summary>Adiciona um contato (e-mail, telefone, celular ou WhatsApp).</summary>
    [HttpPost]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(ContatoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContatoDto>> Criar(Guid clienteId, [FromBody] ContatoRequest request, CancellationToken cancellationToken)
    {
        var criado = await _service.CriarAsync(clienteId, request, cancellationToken);
        return Created($"/api/v1/clientes/{clienteId}/contatos/{criado.Id}", criado);
    }

    /// <summary>Atualiza um contato.</summary>
    [HttpPut("{contatoId:guid}")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(ContatoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ContatoDto>> Atualizar(Guid clienteId, Guid contatoId, [FromBody] ContatoRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AtualizarAsync(clienteId, contatoId, request, cancellationToken));

    /// <summary>Remove um contato.</summary>
    [HttpDelete("{contatoId:guid}")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover(Guid clienteId, Guid contatoId, CancellationToken cancellationToken)
    {
        await _service.RemoverAsync(clienteId, contatoId, cancellationToken);
        return NoContent();
    }
}
