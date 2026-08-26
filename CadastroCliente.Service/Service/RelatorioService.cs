using System.Text;
using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.CrossCutting.Helper;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Domain.Query;

namespace CadastroCliente.Service.Service;

public sealed class RelatorioService : IRelatorioAppService
{
    private readonly IClienteRepository _repository;

    public RelatorioService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<RelatorioResumoDto> ObterResumoAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _repository.GetEstatisticasAsync(cancellationToken);
        return new RelatorioResumoDto
        {
            Total = stats.Total,
            Ativos = stats.Ativos,
            Inativos = stats.Inativos,
            Bloqueados = stats.Bloqueados,
            PessoasFisicas = stats.PessoasFisicas,
            PessoasJuridicas = stats.PessoasJuridicas,
            Excluidos = stats.Excluidos,
            PorUf = stats.PorUf
        };
    }

    public async Task<string> ExportarCsvAsync(CancellationToken cancellationToken = default)
    {
        var pagina = await _repository.SearchAsync(new ClienteFiltro { Pagina = 1, TamanhoPagina = 100 }, cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Id;Nome;TipoPessoa;Documento;Status;Cidade;Uf;CriadoEm");
        foreach (var cliente in pagina.Itens)
        {
            var endereco = cliente.Enderecos.FirstOrDefault(e => e.Principal) ?? cliente.Enderecos.FirstOrDefault();
            sb.AppendLine(string.Join(';',
                cliente.Id,
                Escape(cliente.Nome),
                cliente.TipoPessoa,
                DocumentoHelper.Formatado(cliente.Documento, cliente.TipoPessoa),
                cliente.Status,
                Escape(endereco?.Cidade),
                endereco?.Uf,
                cliente.CriadoEm.ToString("o")));
        }

        return sb.ToString();
    }

    private static string Escape(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
        {
            return string.Empty;
        }

        return valor.Contains(';') || valor.Contains('"')
            ? $"\"{valor.Replace("\"", "\"\"")}\""
            : valor;
    }
}
