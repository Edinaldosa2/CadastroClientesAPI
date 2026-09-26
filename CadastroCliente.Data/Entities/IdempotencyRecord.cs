namespace CadastroCliente.Data.Entities;

public sealed class IdempotencyRecord
{
    public string Key { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string ContentType { get; set; } = "application/json";
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
