namespace CadastroCliente.Aplicacao.DTOs;

public sealed class RelatorioResumoDto
{
    public long Total { get; init; }
    public long Ativos { get; init; }
    public long Inativos { get; init; }
    public long Bloqueados { get; init; }
    public long PessoasFisicas { get; init; }
    public long PessoasJuridicas { get; init; }
    public long Excluidos { get; init; }
    public IReadOnlyDictionary<string, long> PorUf { get; init; } = new Dictionary<string, long>();
}
