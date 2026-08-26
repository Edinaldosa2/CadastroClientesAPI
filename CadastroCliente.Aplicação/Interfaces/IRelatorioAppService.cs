using CadastroCliente.Aplicacao.DTOs;

namespace CadastroCliente.Aplicacao.Interfaces;

public interface IRelatorioAppService
{
    Task<RelatorioResumoDto> ObterResumoAsync(CancellationToken cancellationToken = default);
    Task<string> ExportarCsvAsync(CancellationToken cancellationToken = default);
}
