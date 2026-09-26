# Architecture

CadastroClientes is a layered .NET 8 API. Dependencies point inward: the HTTP host and infrastructure depend on application contracts; the domain has no framework references.

```
CadastroClientes (API)
        │  JWT, ICurrentUser, correlation
        ▼
CadastroCliente.Service
        │  use cases + domain event dispatcher
        ├── CadastroCliente.Aplicacao   ports, DTOs, validators
        ├── CadastroCliente.Data        EF Core / SQLite + auditoria
        ├── CadastroCliente.Domain      entities, events, value objects
        └── Cadastro.CrossCutting       CPF/CNPJ/CEP/phone, clock
```

## Data flow

1. Controller binds JSON to a request DTO and enforces JWT roles.
2. Application service runs FluentValidation.
3. Domain entity enforces invariants (CPF/CNPJ, status, nested limits, unlock reason) and raises domain events.
4. `DispatchingUnitOfWork` persists SQLite, then dispatches events. A handler writes the audit trail. Physical delete of a customer throws.
5. Mapper returns response DTOs. Failures become RFC 7807 Problem Details with `code`.

## Domain events

`Cliente` raises in-process events on create, update, status change, soft delete and restore. After `SaveChanges`, `DomainEventDispatcher` notifies handlers. The first handler persists `auditoria` with the authenticated user and correlation id. Extra handlers can be registered without changing the controllers.

## Extension points

| Need | Where to change |
|------|-----------------|
| SQL Server / PostgreSQL | `AddDbContext` in `Program.cs` and the matching EF provider package |
| Extra customer fields | `Cliente` entity + mapping + DTO + validator |
| Authentication users | `Users` and `Jwt` in configuration; `TokenService` |
| Extra event handler | Implement `IDomainEventHandler` and register it in `AddCadastroServices` |
| Outbox / message bus | Replace the in-process dispatcher with a queued publisher |
| Multi-tenant | Add `TenantId` to `EntityBase` and a global query filter |

## Persistence notes

- Primary keys are application-assigned GUIDs stored as TEXT on SQLite.
- New nested entities (address/contact) must be `RegisterNew`'d so EF inserts instead of updating a missing row.
- `HasQueryFilter(c => !c.Excluido)` hides soft-deleted customers. Pass `incluirExcluidos=true` (escrita) or call restore.
- Idempotency keys live in `idempotency_keys` with the request body hash.

## HTTP cross-cutting

- JWT Bearer (`leitura` / `escrita`) on customer, address, contact and report routes.
- `ExceptionHandlingMiddleware` maps domain exceptions to 404/409/412/422/400/500 and never leaks stack traces in Production.
- `RequestLoggingMiddleware` scopes method/path; documents are masked via `DocumentoHelper.Mascarado`.
- `CorrelationIdMiddleware` honors and echoes `X-Correlation-Id`.
- `ApiKeyMiddleware` is a no-op until `Security:ApiKey` is set (catalog, health and `/auth` stay anonymous).
- `Idempotent` resource filter persists successful POSTs and rejects hash mismatch.
- Production rejects weak `Jwt:Key` and SQLite InMemory. CORS uses `Security:AllowedOrigins`.
- Docker image runs as uid `10001` with a read-only root filesystem.
