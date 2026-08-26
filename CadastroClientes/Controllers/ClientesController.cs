using Asp.Versioning;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Query;
using CadastroClientes.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class ClientesController : ControllerBase
{
    private readonly IClienteAppService _service;

    public ClientesController(IClienteAppService service)
    {
        _service = service;
    }

    /// <summary>Lista clientes com paginação, filtros e ordenação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClienteListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ClienteListItemDto>>> Listar(
        [FromQuery] string? nome,
        [FromQuery] string? documento,
        [FromQuery] TipoPessoa? tipoPessoa,
        [FromQuery] StatusCliente? status,
        [FromQuery] string? cidade,
        [FromQuery] string? uf,
        [FromQuery] bool incluirExcluidos = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        [FromQuery] string ordenarPor = "nome",
        [FromQuery] bool descendente = false,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _service.ListarAsync(new ClienteFiltro
        {
            Nome = nome,
            Documento = documento,
            TipoPessoa = tipoPessoa,
            Status = status,
            Cidade = cidade,
            Uf = uf,
            IncluirExcluidos = incluirExcluidos,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            OrdenarPor = ordenarPor,
            Descendente = descendente
        }, cancellationToken);

        Response.Headers["X-Total-Count"] = resultado.Total.ToString();
        Response.Headers["X-Page"] = resultado.Pagina.ToString();
        Response.Headers["X-Page-Size"] = resultado.TamanhoPagina.ToString();
        return Ok(resultado);
    }

    /// <summary>Obtém um cliente pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> ObterPorId(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.ObterPorIdAsync(id, cancellationToken));

    /// <summary>Obtém um cliente pelo CPF ou CNPJ.</summary>
    [HttpGet("documento/{documento}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> ObterPorDocumento(string documento, CancellationToken cancellationToken)
        => Ok(await _service.ObterPorDocumentoAsync(documento, cancellationToken));

    /// <summary>Cria um novo cliente. Envie Idempotency-Key para repetir o POST com segurança.</summary>
    [HttpPost]
    [Idempotent]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ClienteDto>> Criar([FromBody] CriarClienteRequest request, CancellationToken cancellationToken)
    {
        var criado = await _service.CriarAsync(request, cancellationToken);
        return Created($"/api/v1/clientes/{criado.Id}", criado);
    }

    /// <summary>Substitui os dados cadastrais do cliente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> Atualizar(Guid id, [FromBody] AtualizarClienteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AtualizarAsync(id, request, cancellationToken));

    /// <summary>Atualiza parcialmente os dados cadastrais do cliente.</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> Patch(Guid id, [FromBody] PatchClienteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.PatchAsync(id, request, cancellationToken));

    /// <summary>Exclui logicamente um cliente.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await _service.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Reativa um cliente excluído.</summary>
    [HttpPost("{id:guid}/restaurar")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteDto>> Restaurar(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.RestaurarAsync(id, cancellationToken));

    /// <summary>Ativa um cliente inativo ou bloqueado.</summary>
    [HttpPost("{id:guid}/ativar")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteDto>> Ativar(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.AtivarAsync(id, cancellationToken));

    /// <summary>Inativa um cliente ativo.</summary>
    [HttpPost("{id:guid}/inativar")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteDto>> Inativar(Guid id, [FromBody] AlterarStatusRequest? request, CancellationToken cancellationToken)
        => Ok(await _service.InativarAsync(id, request ?? new AlterarStatusRequest(), cancellationToken));

    /// <summary>Bloqueia um cliente. Cadastro deixa de aceitar edições.</summary>
    [HttpPost("{id:guid}/bloquear")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteDto>> Bloquear(Guid id, [FromBody] AlterarStatusRequest? request, CancellationToken cancellationToken)
        => Ok(await _service.BloquearAsync(id, request ?? new AlterarStatusRequest(), cancellationToken));
}
