# Architecture

CadastroClientes is a layered .NET 8 API. Dependencies point inward: the HTTP host and infrastructure depend on application contracts; the domain has no framework references.

```
CadastroClientes (API)
        │
        ▼
CadastroCliente.Service  ──► CadastroCliente.Aplicacao (DTOs, validators, ports)
        │
        ├── CadastroCliente.Data (EF Core / SQLite)
        ├── CadastroCliente.Domain (entities, value objects, rules)
        └── Cadastro.CrossCutting (document helpers, clock)
```

## Data flow

1. Controller binds JSON to a request DTO.
2. Application service runs FluentValidation.
3. Domain entity enforces invariants (CPF/CNPJ, status, nested limits).
4. Repository + unit of work persist SQLite.
5. Mapper returns response DTOs. Failures become RFC 7807 Problem Details.

## Extension points

| Need | Where to change |
|------|-----------------|
| SQL Server / PostgreSQL | `AddDbContext` in `Program.cs` and the matching EF provider package |
| Extra customer fields | `Cliente` entity + mapping + DTO + validator |
| Authentication | `ApiKeyMiddleware` today; replace with JWT/OpenIddict in the host |
| Outbox / events | Raise from `Cliente` methods and dispatch after `SaveChangesAsync` |
| Multi-tenant | Add `TenantId` to `EntityBase` and a global query filter |

## Persistence notes

- Primary keys are application-assigned GUIDs stored as TEXT on SQLite.
- New nested entities (address/contact) must be `RegisterNew`'d so EF inserts instead of updating a missing row.
- `HasQueryFilter(c => !c.Excluido)` hides soft-deleted customers. Pass `incluirExcluidos=true` or call restore.

## HTTP cross-cutting

- `ExceptionHandlingMiddleware` maps domain exceptions to 404/409/422/400/500.
- `CorrelationIdMiddleware` honors and echoes `X-Correlation-Id`.
- `ApiKeyMiddleware` is a no-op until `Security:ApiKey` is set.
- `Idempotent` resource filter replays successful POSTs with the same key and body hash.
