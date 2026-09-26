using CadastroCliente.Data.Context;
using CadastroCliente.Data.Entities;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Domain.Query;
using Microsoft.EntityFrameworkCore;

namespace CadastroCliente.Data.Implementation;

public sealed class AuditoriaRepository : IAuditoriaRepository
{
    private readonly CadastroClienteContext _context;

    public AuditoriaRepository(CadastroClienteContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditoriaItem item, CancellationToken cancellationToken = default)
    {
        await _context.Auditorias.AddAsync(new AuditoriaRecord
        {
            Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
            ClienteId = item.ClienteId,
            Tipo = item.Tipo,
            Descricao = item.Descricao,
            Usuario = item.Usuario,
            CorrelationId = item.CorrelationId,
            OcorridoEm = item.OcorridoEm.UtcDateTime
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditoriaItem>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Auditorias.AsNoTracking()
            .Where(x => x.ClienteId == clienteId)
            .OrderBy(x => x.OcorridoEm)
            .ToListAsync(cancellationToken);

        return rows.Select(x => new AuditoriaItem
        {
            Id = x.Id,
            ClienteId = x.ClienteId,
            Tipo = x.Tipo,
            Descricao = x.Descricao,
            Usuario = x.Usuario,
            CorrelationId = x.CorrelationId,
            OcorridoEm = new DateTimeOffset(x.OcorridoEm, TimeSpan.Zero)
        }).ToList();
    }
}
