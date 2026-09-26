namespace CadastroCliente.Aplicacao.DTOs;

public sealed class AuditoriaDto
{
    public Guid Id { get; init; }
    public Guid ClienteId { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string? Usuario { get; init; }
    public string? CorrelationId { get; init; }
    public DateTimeOffset OcorridoEm { get; init; }
}
