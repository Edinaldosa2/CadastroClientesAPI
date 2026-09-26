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
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return rows.Select(Mapear).ToList();
    }

    public async Task<PagedResult<AuditoriaItem>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanhoPagina = tamanhoPagina switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => tamanhoPagina
        };

        var query = _context.Auditorias.AsNoTracking()
            .OrderByDescending(x => x.OcorridoEm)
            .ThenByDescending(x => x.Id);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditoriaItem>(rows.Select(Mapear).ToList(), pagina, tamanhoPagina, total);
    }

    private static AuditoriaItem Mapear(AuditoriaRecord x) => new()
    {
        Id = x.Id,
        ClienteId = x.ClienteId,
        Tipo = x.Tipo,
        Descricao = x.Descricao,
        Usuario = x.Usuario,
        CorrelationId = x.CorrelationId,
        OcorridoEm = new DateTimeOffset(x.OcorridoEm, TimeSpan.Zero)
    };
}
