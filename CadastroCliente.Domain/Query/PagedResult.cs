namespace CadastroCliente.Domain.Query;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Itens { get; }
    public int Pagina { get; }
    public int TamanhoPagina { get; }
    public long Total { get; }
    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanhoPagina);
    public bool TemProxima => Pagina < TotalPaginas;
    public bool TemAnterior => Pagina > 1;

    public PagedResult(IReadOnlyList<T> itens, int pagina, int tamanhoPagina, long total)
    {
        Itens = itens;
        Pagina = pagina;
        TamanhoPagina = tamanhoPagina;
        Total = total;
    }
}
