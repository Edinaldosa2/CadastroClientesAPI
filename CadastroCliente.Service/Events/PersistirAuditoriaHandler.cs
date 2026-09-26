using CadastroCliente.Aplicacao.Abstractions;
using CadastroCliente.Domain.Events;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Domain.Query;

namespace CadastroCliente.Service.Events;

public sealed class PersistirAuditoriaHandler : IDomainEventHandler
{
    private readonly IAuditoriaRepository _auditoria;
    private readonly ICurrentUser _user;
    private readonly ICorrelationContext _correlation;

    public PersistirAuditoriaHandler(
        IAuditoriaRepository auditoria,
        ICurrentUser user,
        ICorrelationContext correlation)
    {
        _auditoria = auditoria;
        _user = user;
        _correlation = correlation;
    }

    public Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var (clienteId, tipo, descricao) = domainEvent switch
        {
            ClienteCriado e => (e.ClienteId, "cliente.criado", $"Cliente {e.Nome} cadastrado."),
            ClienteAtualizado e => (e.ClienteId, "cliente.atualizado", $"Dados cadastrais de {e.Nome} atualizados."),
            ClienteStatusAlterado e => (e.ClienteId, "cliente.status_alterado",
                $"Status {e.De} → {e.Para}{(string.IsNullOrWhiteSpace(e.Motivo) ? string.Empty : $": {e.Motivo}")}."),
            ClienteExcluido e => (e.ClienteId, "cliente.excluido", "Cliente excluído logicamente."),
            ClienteRestaurado e => (e.ClienteId, "cliente.restaurado", "Cliente restaurado."),
            _ => (Guid.Empty, string.Empty, string.Empty)
        };

        if (clienteId == Guid.Empty)
        {
            return Task.CompletedTask;
        }

        return _auditoria.AddAsync(new AuditoriaItem
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Tipo = tipo,
            Descricao = descricao,
            Usuario = _user.Usuario,
            CorrelationId = _correlation.CorrelationId,
            OcorridoEm = domainEvent.OcorridoEm
        }, cancellationToken);
    }
}
