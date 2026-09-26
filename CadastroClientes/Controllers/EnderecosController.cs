using Asp.Versioning;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Controllers;

[ApiController]
[Authorize(Roles = "leitura,escrita")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes/{clienteId:guid}/enderecos")]
[Produces("application/json")]
public sealed class EnderecosController : ControllerBase
{
    private readonly IEnderecoAppService _service;

    public EnderecosController(IEnderecoAppService service)
    {
        _service = service;
    }

    /// <summary>Lista os endereços do cliente.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EnderecoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EnderecoDto>>> Listar(Guid clienteId, CancellationToken cancellationToken)
        => Ok(await _service.ListarAsync(clienteId, cancellationToken));

    /// <summary>Obtém um endereço específico.</summary>
    [HttpGet("{enderecoId:guid}")]
    [ProducesResponseType(typeof(EnderecoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnderecoDto>> Obter(Guid clienteId, Guid enderecoId, CancellationToken cancellationToken)
        => Ok(await _service.ObterAsync(clienteId, enderecoId, cancellationToken));

    /// <summary>Adiciona um endereço ao cliente.</summary>
    [HttpPost]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(EnderecoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EnderecoDto>> Criar(Guid clienteId, [FromBody] EnderecoRequest request, CancellationToken cancellationToken)
    {
        var criado = await _service.CriarAsync(clienteId, request, cancellationToken);
        return Created($"/api/v1/clientes/{clienteId}/enderecos/{criado.Id}", criado);
    }

    /// <summary>Atualiza um endereço.</summary>
    [HttpPut("{enderecoId:guid}")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(EnderecoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnderecoDto>> Atualizar(Guid clienteId, Guid enderecoId, [FromBody] EnderecoRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AtualizarAsync(clienteId, enderecoId, request, cancellationToken));

    /// <summary>Define o endereço como principal.</summary>
    [HttpPatch("{enderecoId:guid}/principal")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(EnderecoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnderecoDto>> Principal(Guid clienteId, Guid enderecoId, CancellationToken cancellationToken)
        => Ok(await _service.DefinirPrincipalAsync(clienteId, enderecoId, cancellationToken));

    /// <summary>Remove um endereço.</summary>
    [HttpDelete("{enderecoId:guid}")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover(Guid clienteId, Guid enderecoId, CancellationToken cancellationToken)
    {
        await _service.RemoverAsync(clienteId, enderecoId, cancellationToken);
        return NoContent();
    }
}
