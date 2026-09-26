using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Domain.Events;

public sealed record ClienteCriado(Guid ClienteId, string Nome, string Documento, DateTimeOffset OcorridoEm) : IDomainEvent;

public sealed record ClienteAtualizado(Guid ClienteId, string Nome, DateTimeOffset OcorridoEm) : IDomainEvent;

public sealed record ClienteStatusAlterado(
    Guid ClienteId,
    StatusCliente De,
    StatusCliente Para,
    string? Motivo,
    DateTimeOffset OcorridoEm) : IDomainEvent;

public sealed record ClienteExcluido(Guid ClienteId, DateTimeOffset OcorridoEm) : IDomainEvent;

public sealed record ClienteRestaurado(Guid ClienteId, DateTimeOffset OcorridoEm) : IDomainEvent;
