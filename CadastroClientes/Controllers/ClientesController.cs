using Asp.Versioning;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.Query;
using CadastroClientes.Infrastructure;
using CadastroClientes.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Controllers;

[ApiController]
[Authorize(Roles = "leitura,escrita")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class ClientesController : ControllerBase
{
    private readonly IClienteAppService _service;
    private readonly IAuditoriaAppService _auditoria;

    public ClientesController(IClienteAppService service, IAuditoriaAppService auditoria)
    {
        _service = service;
        _auditoria = auditoria;
    }

    /// <summary>Lista clientes com paginação, cursor, filtros e ordenação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClienteListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ClienteListItemDto>>> Listar(
        [FromQuery] string? nome,
        [FromQuery] string? documento,
        [FromQuery] TipoPessoa? tipoPessoa,
        [FromQuery] StatusCliente? status,
        [FromQuery] string? cidade,
        [FromQuery] string? uf,
        [FromQuery] string? contato,
        [FromQuery] DateTimeOffset? criadoDe,
        [FromQuery] DateTimeOffset? criadoAte,
        [FromQuery] Guid? depoisDe,
        [FromQuery] bool incluirExcluidos = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        [FromQuery] string ordenarPor = "nome",
        [FromQuery] bool descendente = false,
        CancellationToken cancellationToken = default)
    {
        if (incluirExcluidos && !User.IsInRole("escrita"))
        {
            return Forbid();
        }

        var whitelist = new[] { "nome", "documento", "status", "criadoem", "atualizadoem" };
        if (!whitelist.Contains(ordenarPor.Trim().ToLowerInvariant()))
        {
            ordenarPor = "nome";
        }

        var resultado = await _service.ListarAsync(new ClienteFiltro
        {
            Nome = nome,
            Documento = documento,
            TipoPessoa = tipoPessoa,
            Status = status,
            Cidade = cidade,
            Uf = uf,
            Contato = contato,
            CriadoDe = criadoDe,
            CriadoAte = criadoAte,
            DepoisDe = depoisDe,
            IncluirExcluidos = incluirExcluidos,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            OrdenarPor = ordenarPor,
            Descendente = descendente
        }, cancellationToken);

        Response.Headers["X-Total-Count"] = resultado.Total.ToString();
        Response.Headers["X-Page"] = resultado.Pagina.ToString();
        Response.Headers["X-Page-Size"] = resultado.TamanhoPagina.ToString();
        if (!string.IsNullOrWhiteSpace(resultado.ProximoCursor))
        {
            Response.Headers["X-Next-Cursor"] = resultado.ProximoCursor;
        }

        return Ok(resultado);
    }

    /// <summary>Obtém um cliente pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        GarantirId(id);
        var dto = await _service.ObterPorIdAsync(id, cancellationToken);
        var etag = EtagHelper.From(dto);
        Response.Headers.ETag = etag;
        return Ok(dto);
    }

    /// <summary>Lista a trilha de auditoria do cliente.</summary>
    [HttpGet("{id:guid}/auditoria")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AuditoriaDto>>> Auditoria(Guid id, CancellationToken cancellationToken)
    {
        GarantirId(id);
        return Ok(await _auditoria.ListarPorClienteAsync(id, cancellationToken));
    }

    /// <summary>Obtém um cliente pelo CPF ou CNPJ, com ou sem máscara.</summary>
    [HttpGet("documento/{documento}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> ObterPorDocumento(string documento, CancellationToken cancellationToken)
        => Ok(await _service.ObterPorDocumentoAsync(documento, cancellationToken));

    /// <summary>Cria um novo cliente. Envie Idempotency-Key para repetir o POST com segurança.</summary>
    [HttpPost]
    [Authorize(Roles = "escrita")]
    [Idempotent]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ClienteDto>> Criar([FromBody] CriarClienteRequest request, CancellationToken cancellationToken)
    {
        var criado = await _service.CriarAsync(request, cancellationToken);
        return Created($"/api/v1/clientes/{criado.Id}", criado);
    }

    [HttpDelete]
    [Authorize(Roles = "escrita")]
    public IActionResult ExcluirColecao() => StatusCode(StatusCodes.Status405MethodNotAllowed);

    /// <summary>Substitui os dados cadastrais do cliente.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> Atualizar(Guid id, [FromBody] AtualizarClienteRequest request, CancellationToken cancellationToken)
    {
        GarantirId(id);
        await GarantirEtag(id, cancellationToken);
        var dto = await _service.AtualizarAsync(id, request, cancellationToken);
        Response.Headers.ETag = EtagHelper.From(dto);
        return Ok(dto);
    }

    /// <summary>Atualiza parcialmente os dados cadastrais do cliente.</summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> Patch(Guid id, [FromBody] PatchClienteRequest request, CancellationToken cancellationToken)
    {
        GarantirId(id);
        await GarantirEtag(id, cancellationToken);
        var dto = await _service.PatchAsync(id, request, cancellationToken);
        Response.Headers.ETag = EtagHelper.From(dto);
        return Ok(dto);
    }

    /// <summary>Exclui logicamente um cliente.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        GarantirId(id);
        await _service.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Reativa um cliente excluído.</summary>
    [HttpPost("{id:guid}/restaurar")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteDto>> Restaurar(Guid id, CancellationToken cancellationToken)
    {
        GarantirId(id);
        return Ok(await _service.RestaurarAsync(id, cancellationToken));
    }

    /// <summary>Ativa um cliente inativo ou bloqueado. Bloqueado exige motivo.</summary>
    [HttpPost("{id:guid}/ativar")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteDto>> Ativar(Guid id, [FromBody] AlterarStatusRequest? request, CancellationToken cancellationToken)
    {
        GarantirId(id);
        return Ok(await _service.AtivarAsync(id, request, cancellationToken));
    }

    /// <summary>Inativa um cliente ativo.</summary>
    [HttpPost("{id:guid}/inativar")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteDto>> Inativar(Guid id, [FromBody] AlterarStatusRequest? request, CancellationToken cancellationToken)
    {
        GarantirId(id);
        return Ok(await _service.InativarAsync(id, request ?? new AlterarStatusRequest(), cancellationToken));
    }

    /// <summary>Bloqueia um cliente. Cadastro deixa de aceitar edições.</summary>
    [HttpPost("{id:guid}/bloquear")]
    [Authorize(Roles = "escrita")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteDto>> Bloquear(Guid id, [FromBody] AlterarStatusRequest? request, CancellationToken cancellationToken)
    {
        GarantirId(id);
        return Ok(await _service.BloquearAsync(id, request ?? new AlterarStatusRequest(), cancellationToken));
    }

    private static void GarantirId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessRuleException("id_invalido", "id", "Identificador inválido.");
        }
    }

    private async Task GarantirEtag(Guid id, CancellationToken cancellationToken)
    {
        if (!Request.Headers.IfMatch.Any())
        {
            return;
        }

        var atual = await _service.ObterPorIdAsync(id, cancellationToken);
        var etag = EtagHelper.From(atual);
        if (!EtagHelper.Matches(Request.Headers.IfMatch.ToString(), etag))
        {
            throw new PreconditionFailedException("ETag não confere. Recarregue o cliente e envie If-Match.");
        }
    }
}
