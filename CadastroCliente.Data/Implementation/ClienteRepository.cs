using CadastroCliente.Data.Context;
using CadastroCliente.Data.Repository;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Domain.Query;
using CadastroCliente.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CadastroCliente.Data.Implementation;

public sealed class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(CadastroClienteContext context) : base(context)
    {
    }

    public Task<Cliente?> GetByIdAsync(Guid id, bool incluirExcluidos = false, CancellationToken cancellationToken = default)
    {
        var query = QueryComIncludes();
        if (incluirExcluidos)
        {
            query = query.IgnoreQueryFilters();
        }

        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Cliente?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        var digits = Documento.SomenteDigitos(documento);
        return QueryComIncludes().FirstOrDefaultAsync(x => x.Documento == digits, cancellationToken);
    }

    public Task<bool> ExistsDocumentoAsync(string documento, Guid? ignoreId = null, CancellationToken cancellationToken = default)
    {
        var digits = Documento.SomenteDigitos(documento);
        var query = Set.IgnoreQueryFilters().AsQueryable();
        if (ignoreId.HasValue)
        {
            query = query.Where(x => x.Id != ignoreId.Value);
        }

        return query.AnyAsync(x => x.Documento == digits, cancellationToken);
    }

    public async Task<PagedResult<Cliente>> SearchAsync(ClienteFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = QueryComIncludes();
        if (filtro.IncluirExcluidos)
        {
            query = query.IgnoreQueryFilters();
        }

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            var nome = filtro.Nome.Trim().ToLower();
            query = query.Where(x => x.Nome.ToLower().Contains(nome) || (x.NomeFantasia != null && x.NomeFantasia.ToLower().Contains(nome)));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Documento))
        {
            var digits = Documento.SomenteDigitos(filtro.Documento);
            query = query.Where(x => x.Documento.Contains(digits));
        }

        if (filtro.TipoPessoa.HasValue)
        {
            query = query.Where(x => x.TipoPessoa == filtro.TipoPessoa.Value);
        }

        if (filtro.Status.HasValue)
        {
            query = query.Where(x => x.Status == filtro.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Cidade))
        {
            var cidade = filtro.Cidade.Trim();
            query = query.Where(x => x.Enderecos.Any(e => e.Cidade.Contains(cidade)));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Uf))
        {
            var uf = filtro.Uf.Trim().ToUpper();
            query = query.Where(x => x.Enderecos.Any(e => e.Uf == uf));
        }

        query = Ordenar(query, filtro.OrdenarPor, filtro.Descendente);

        var total = await query.CountAsync(cancellationToken);
        var pagina = filtro.PaginaNormalizada;
        var tamanho = filtro.TamanhoNormalizado;
        var itens = await query
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(cancellationToken);

        return new PagedResult<Cliente>(itens, pagina, tamanho, total);
    }

    public async Task<ClienteEstatisticas> GetEstatisticasAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await Set.IgnoreQueryFilters().AsNoTracking().ToListAsync(cancellationToken);
        var ativos = clientes.Where(x => !x.Excluido).ToList();
        var enderecos = await Context.Enderecos.AsNoTracking().ToListAsync(cancellationToken);

        var porUf = enderecos
            .Where(e => e.Principal)
            .GroupBy(e => e.Uf)
            .ToDictionary(g => g.Key, g => (long)g.Count());

        return new ClienteEstatisticas
        {
            Total = ativos.Count,
            Ativos = ativos.Count(x => x.Status == StatusCliente.Ativo),
            Inativos = ativos.Count(x => x.Status == StatusCliente.Inativo),
            Bloqueados = ativos.Count(x => x.Status == StatusCliente.Bloqueado),
            PessoasFisicas = ativos.Count(x => x.TipoPessoa == TipoPessoa.Fisica),
            PessoasJuridicas = ativos.Count(x => x.TipoPessoa == TipoPessoa.Juridica),
            Excluidos = clientes.Count(x => x.Excluido),
            PorUf = porUf,
            PorStatus = ativos.GroupBy(x => x.Status).ToDictionary(g => g.Key, g => (long)g.Count())
        };
    }

    public async Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(cliente, cancellationToken);
    }

    public void Remove(Cliente cliente) => Set.Remove(cliente);

    private IQueryable<Cliente> QueryComIncludes()
        => Set.Include(x => x.Enderecos).Include(x => x.Contatos);

    private static IQueryable<Cliente> Ordenar(IQueryable<Cliente> query, string? campo, bool descendente)
    {
        return (campo?.Trim().ToLowerInvariant()) switch
        {
            "documento" => descendente ? query.OrderByDescending(x => x.Documento) : query.OrderBy(x => x.Documento),
            "status" => descendente ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "criadoem" => descendente ? query.OrderByDescending(x => x.CriadoEm) : query.OrderBy(x => x.CriadoEm),
            "atualizadoem" => descendente ? query.OrderByDescending(x => x.AtualizadoEm) : query.OrderBy(x => x.AtualizadoEm),
            _ => descendente ? query.OrderByDescending(x => x.Nome) : query.OrderBy(x => x.Nome)
        };
    }
}
