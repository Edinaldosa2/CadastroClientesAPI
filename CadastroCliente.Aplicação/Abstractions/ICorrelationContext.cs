namespace CadastroCliente.Aplicacao.Abstractions;

public interface ICorrelationContext
{
    string? CorrelationId { get; }
}
