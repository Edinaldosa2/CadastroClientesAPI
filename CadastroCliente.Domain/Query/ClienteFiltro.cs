using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Domain.Query;

public sealed class ClienteFiltro
{
    public string? Nome { get; init; }
    public string? Documento { get; init; }
    public TipoPessoa? TipoPessoa { get; init; }
    public StatusCliente? Status { get; init; }
    public string? Cidade { get; init; }
    public string? Uf { get; init; }
    public string? Contato { get; init; }
    public DateTimeOffset? CriadoDe { get; init; }
    public DateTimeOffset? CriadoAte { get; init; }
    public Guid? DepoisDe { get; init; }
    public bool IncluirExcluidos { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamanhoPagina { get; init; } = 20;
    public string OrdenarPor { get; init; } = "nome";
    public bool Descendente { get; init; }

    public int PaginaNormalizada => Pagina < 1 ? 1 : Pagina;
    public int TamanhoNormalizado => TamanhoPagina switch
    {
        < 1 => 20,
        > 100 => 100,
        _ => TamanhoPagina
    };
}
