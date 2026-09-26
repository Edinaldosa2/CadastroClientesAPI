using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.Interfaces;

namespace CadastroCliente.Service.Service;

public sealed class AuditoriaService : IAuditoriaAppService
{
    private readonly IClienteRepository _clientes;
    private readonly IAuditoriaRepository _auditoria;

    public AuditoriaService(IClienteRepository clientes, IAuditoriaRepository auditoria)
    {
        _clientes = clientes;
        _auditoria = auditoria;
    }

    public async Task<IReadOnlyList<AuditoriaDto>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.GetByIdAsync(clienteId, incluirExcluidos: true, cancellationToken)
                      ?? throw new NotFoundException("Cliente", clienteId);

        var itens = await _auditoria.ListarPorClienteAsync(cliente.Id, cancellationToken);
        return itens.Select(x => new AuditoriaDto
        {
            Id = x.Id,
            ClienteId = x.ClienteId,
            Tipo = x.Tipo,
            Descricao = x.Descricao,
            Usuario = x.Usuario,
            CorrelationId = x.CorrelationId,
            OcorridoEm = x.OcorridoEm
        }).ToList();
    }
}
