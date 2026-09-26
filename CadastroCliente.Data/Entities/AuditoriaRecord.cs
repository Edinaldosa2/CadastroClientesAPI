namespace CadastroCliente.Data.Entities;

public sealed class AuditoriaRecord
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? Usuario { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime OcorridoEm { get; set; }
}
