namespace CadastroCliente.Domain.Query;

public sealed class AuditoriaItem
{
    public Guid Id { get; init; }
    public Guid ClienteId { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string? Usuario { get; init; }
    public string? CorrelationId { get; init; }
    public DateTimeOffset OcorridoEm { get; init; }
}
