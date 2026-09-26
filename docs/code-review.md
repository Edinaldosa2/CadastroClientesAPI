# Code review (original skeleton)

The repository started as an empty ASP.NET Core 7 Razor Pages template with DDD folder names and no runnable API. This document records the review and how v1.0.0 addressed it.

## Findings

| Severity | Finding |
|----------|---------|
| Blocker | `Program.cs` only mapped Razor Pages. There was no REST pipeline, Swagger or JSON API. |
| Blocker | `CadastroClienteController` was an empty class. No routes existed. |
| Blocker | Domain, Data, Service and Application projects were empty stubs (`Class1`, unused `using`s). They could not compile into a use case. |
| Blocker | No EF Core package, no connection string, no entities. Persistence could not run. |
| High | Target framework was `net7.0` (out of support). |
| High | Test project referenced xUnit but contained no tests. |
| High | `Startup.cs` was unused next to the minimal hosting model. |
| Medium | Solution mixed leftover `Negocio` / `Modelo` / `Util` / `Arquitetura` placeholders with the intended layers. |
| Medium | Web project shipped jQuery/Bootstrap `wwwroot` assets that do not belong in an API. |
| Medium | Dockerfile targeted Windows containers (`DockerDefaultTargetOS`). |
| Low | README was a single heading. |
| Low | Mapping class typo `ClienttMap`. |

## What changed

- Upgraded to .NET 8 LTS and converted the host to a versioned REST API.
- Implemented customer / address / contact aggregates with CPF, CNPJ, CEP and UF rules.
- SQLite + EF Core with seed data so `dotnet run` is enough to use the API.
- Professional routes: pagination, document lookup, lifecycle, nested resources, reports, health, catalog.
- Cross-cutting: Problem Details, correlation id, optional API key, rate limit, idempotent POST.
- Test coverage across domain, helpers, validators, services, repository and HTTP (~95% line coverage).
- README, architecture notes, HTTP samples, MIT license, CI and Linux Docker/compose.

## Remaining product choices (not defects)

- JWT users are configured in `appsettings` (demo credentials). Swap `TokenService` for an identity provider when needed.
- Optional `X-Api-Key` remains as a second gate when `Security:ApiKey` is set.
- SQLite is the default engine; SQL Server is a connection-string + provider swap.
