using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Domain.Query;

public sealed class ClienteEstatisticas
{
    public long Total { get; init; }
    public long Ativos { get; init; }
    public long Inativos { get; init; }
    public long Bloqueados { get; init; }
    public long PessoasFisicas { get; init; }
    public long PessoasJuridicas { get; init; }
    public long Excluidos { get; init; }
    public IReadOnlyDictionary<string, long> PorUf { get; init; } = new Dictionary<string, long>();
    public IReadOnlyDictionary<StatusCliente, long> PorStatus { get; init; } = new Dictionary<StatusCliente, long>();
}
