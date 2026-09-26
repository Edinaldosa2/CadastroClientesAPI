<p align="center">
  <a href="#us-english">US English</a> |
  <a href="#br-português">BR Português</a>
</p>

---

# Cadastro de Clientes API

**A production-ready REST API for Brazilian customer registration — people, companies, addresses, contacts and operational status.**

CadastroClientes is a **.NET 8** Web API for **CPF/CNPJ-validated customer master data**, **address and contact management**, **activate / inactivate / block workflows**, **soft delete**, **CSV export** and **live health checks**. It runs locally with SQLite — no external database required.

Create a person or company, attach addresses and contacts, search with pagination, and integrate from Swagger or any HTTP client. Document uniqueness, Brazilian ZIP codes, and phone rules are enforced in the domain, not only in the controller.

---

## What's New in v1.2.0

| Area | Update |
|------|--------|
| **Admin role** | JWT profile `admin` receives `admin` + `escrita` + `leitura` and can do every operation |
| **Demo user** | `admin` / `admin-dev` sits beside `editor` (write) and `leitor` (read) |
| **Admin surface** | `GET /api/v1/admin/usuarios` and paginated `GET /api/v1/admin/auditoria` |
| **Reset** | `POST /api/v1/dev/reset` is admin-only (still blocked in Production) |

## What's New in v1.1.0

| Area | Update |
|------|--------|
| **JWT roles** | `POST /api/v1/auth/token` issues `leitura` or `escrita` Bearer tokens |
| **Concurrency** | `ETag` on GET and optional `If-Match` on PUT/PATCH |
| **Query** | Cursor (`depoisDe`), contact and created-at filters, sort whitelist |
| **Idempotency** | Persisted keys + body hash; mismatch returns `409` |
| **Errors** | RFC 7807 `code` (`cpf_invalido`, `documento_imutavel`, `etag_conflito`, …) |
| **Safety** | Immutable document, no physical delete, unlock reason, production JWT/InMemory guards |
| **Architecture** | Domain events after save, current user, correlation and persisted audit trail |
| **Ops** | SQLite file health, demo reset, non-root read-only Docker, CSV UTF-8 BOM |

---

## Table of Contents

- [What's New in v1.0.0](#whats-new-in-v100)
- [Clone from Git](#clone-from-git)
- [Run the API](#run-the-api)
- [First Request](#first-request)
- [Workflows](#workflows)
- [HTTP Routes](#http-routes)
- [Key Features](#key-features)
- [Safety Model](#safety-model)
- [Requirements](#requirements)
- [Repository Layout](#repository-layout)
- [Docker](#docker)
- [Tests](#tests)
- [Documentation](#documentation)
- [License](#license)
- [BR Português](#br-português)

---

<a id="us-english"></a>

## Clone from Git

```bash
git clone https://github.com/Edinaldosa2/CadastroClientesAPI.git
cd CadastroClientesAPI
dotnet restore
dotnet run --project CadastroClientes
```

Open [http://localhost:5105/swagger](http://localhost:5105/swagger).

---

## Run the API

**Option A — .NET SDK (recommended)**

```bash
dotnet run --project CadastroClientes --urls http://localhost:5105
```

**Option B — Docker Compose**

```bash
docker compose up --build
```

The API listens on `http://localhost:8080`. SQLite data is stored in a named volume.

**Option C — Docker only**

```bash
docker build -f CadastroClientes/Dockerfile -t cadastro-clientes-api .
docker run --rm -p 8080:8080 cadastro-clientes-api
```

---

## First Request

The first start creates `cadastro-clientes.dev.db` and seeds two demo customers (Maria Silva and Acme Comércio).

```bash
curl http://localhost:5105/health
curl http://localhost:5105/api
TOKEN=$(curl -s -X POST http://localhost:5105/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"usuario":"editor","senha":"editor-dev"}' | python3 -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")
curl -H "Authorization: Bearer $TOKEN" http://localhost:5105/api/v1/clientes
```

Create a person:

```bash
curl -X POST http://localhost:5105/api/v1/clientes \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: demo-001" \
  -d '{
    "nome": "João da Silva",
    "tipoPessoa": "Fisica",
    "documento": "11144477735",
    "enderecos": [{
      "tipo": "Residencial",
      "logradouro": "Rua Augusta",
      "numero": "200",
      "bairro": "Consolação",
      "cidade": "São Paulo",
      "uf": "SP",
      "cep": "01305000",
      "principal": true
    }]
  }'
```

Demo users: `admin` / `admin-dev` (everything), `editor` / `editor-dev` (write) and `leitor` / `leitor-dev` (read).

Ready-to-run samples: [docs/http/clientes.http](docs/http/clientes.http)

---

## Workflows

| Workflow | Purpose |
|----------|---------|
| **Auth** | Exchange demo credentials for a JWT (`admin`, `escrita` or `leitura`) |
| **Catalog** | Discover resources at `GET /api` |
| **Customers** | Search, create, replace, patch, get by id or document |
| **Lifecycle** | Activate, inactivate, block, soft-delete, restore |
| **Addresses** | Nested CRUD plus mark-as-principal |
| **Contacts** | Email / phone / mobile / WhatsApp per customer |
| **Reports** | Summary counters and CSV export |
| **Ops** | Live/ready health, Swagger, correlation id |

```
Client → /api/v1/clientes  → Application services → Domain rules → SQLite
                              ↳ validation, uniqueness, status machine
```

---

## HTTP Routes

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v1/auth/token` | Issue JWT (`admin` / `escrita` / `leitura`) |
| GET | `/api` | Public catalog |
| GET | `/health` `/health/live` `/health/ready` | Health probes |
| GET | `/api/v1/clientes` | Search (`nome`, `documento`, `contato`, `criadoDe`, `depoisDe`, …) |
| GET | `/api/v1/admin/usuarios` | List configured users (admin; no passwords) |
| GET | `/api/v1/admin/auditoria` | Global audit trail with pagination (admin) |
| POST | `/api/v1/dev/reset` | Recreate demo data (admin; blocked in Production) |
| GET | `/api/v1/clientes/{id}` | Get by id |
| GET | `/api/v1/clientes/documento/{documento}` | Get by CPF/CNPJ |
| POST | `/api/v1/clientes` | Create (optional `Idempotency-Key`) |
| PUT | `/api/v1/clientes/{id}` | Full update of master data |
| PATCH | `/api/v1/clientes/{id}` | Partial update |
| DELETE | `/api/v1/clientes/{id}` | Soft delete |
| POST | `/api/v1/clientes/{id}/ativar` | Activate |
| POST | `/api/v1/clientes/{id}/inativar` | Inactivate |
| POST | `/api/v1/clientes/{id}/bloquear` | Block (edits rejected) |
| POST | `/api/v1/clientes/{id}/restaurar` | Restore a deleted customer |
| GET | `/api/v1/clientes/{id}/auditoria` | Domain-event audit trail |
| GET/POST | `/api/v1/clientes/{id}/enderecos` | List / create addresses |
| GET/PUT/DELETE | `/api/v1/clientes/{id}/enderecos/{enderecoId}` | Address item |
| PATCH | `/api/v1/clientes/{id}/enderecos/{enderecoId}/principal` | Set primary address |
| GET/POST | `/api/v1/clientes/{id}/contatos` | List / create contacts |
| GET/PUT/DELETE | `/api/v1/clientes/{id}/contatos/{contatoId}` | Contact item |
| GET | `/api/v1/relatorios/clientes/resumo` | Counts by status, type and UF |
| GET | `/api/v1/relatorios/clientes/exportar` | CSV download |

Version can also be sent as `X-Api-Version: 1.0` or `?api-version=1.0`.

Errors follow [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807) Problem Details (`application/problem+json`) with `traceId`, stable `code` and field `errors` when validation fails.

---

## Key Features

### Usable API host

- JWT Bearer (`leitura` lists; `escrita` writes, CSV and `incluirExcluidos`; `admin` does everything plus `/admin/*` and reset)
- Swagger UI at `/swagger` (JWT required in Production)
- JSON enums as strings
- Pagination headers `X-Total-Count`, `X-Page`, `X-Page-Size`, `X-Next-Cursor`
- `ETag` / optional `If-Match`
- CORS: any origin locally; `Security:AllowedOrigins` in Production
- 120 requests / minute / IP
- Optional `X-Api-Key` when `Security:ApiKey` is configured
- `X-Correlation-Id` echoed on every response
- Persisted idempotent `POST /clientes` via `Idempotency-Key`

### Domain rules

- Pessoa física (CPF) and jurídica (CNPJ)
- Unique document, even among soft-deleted rows
- Brazilian UF list and 8-digit CEP
- Phone 10–13 digits; e-mail RFC-style
- One principal address; principal contact per type
- Blocked customers cannot be edited
- Soft delete hides the row from default queries

### Persistence

- EF Core + SQLite out of the box
- Design-time factory for migrations
- Demo seed on first run

---

## Safety Model

| Rule | Behavior |
|------|----------|
| JWT on data | Unauthenticated reads of customers/reports return `401` |
| Role split | `leitura` cannot write, export CSV, list deleted rows, manage users or reset |
| Admin gate | `/api/v1/admin/*` and demo reset require `admin`; `escrita` still writes |
| Immutable document | PATCH/extra `documento` returns `422 documento_imutavel` |
| No all-zero docs | CPF/CNPJ with a single repeated digit is invalid |
| No hard delete | Repository `Remove` throws; `DELETE` is soft-delete only |
| No collection delete | `DELETE /api/v1/clientes` returns `405` |
| Blocked gate | Blocked customers reject edits; unlock requires `motivo` |
| Restore clash | Restore fails with `409` if another active row owns the document |
| Client-supplied id | POST `id` is rejected |
| Empty id | `Guid.Empty` returns `422 id_invalido` |
| Stale write | Wrong `If-Match` returns `412 etag_conflito` |
| Idempotency | Same key + different body returns `409 idempotency_conflito` |
| Sort injection | Unknown `ordenarPor` falls back to `nome` |
| InMemory host | SQLite memory is blocked outside `Testing` |
| Weak JWT | Production rejects keys shorter than 32 chars or containing `dev-only` |
| No stack in prod | `500` detail is omitted in Production |
| Demo reset | `POST /api/v1/dev/reset` is `403` in Production |
| Docker | Non-root uid `10001`, read-only root, tmpfs `/tmp` |

---

## Requirements

| Component | Version |
|-----------|---------|
| SDK | [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) |
| OS | Linux, macOS or Windows |
| Database | SQLite (bundled). Swap the connection string for SQL Server if needed |
| Optional | Docker 24+ for compose |

---

## Repository Layout

```
CadastroClientesAPI/
├── CadastroClientes/              HTTP host, controllers, middleware, current user
├── CadastroCliente.Aplicação/     DTOs, validators, ports, abstractions
├── CadastroCliente.Service/       Use cases, event dispatcher, unit of work
├── CadastroCliente.Domain/        Entities, domain events, value objects, rules
├── CadastroCliente.Data/          EF Core, mappings, SQLite
├── Cadastro.CrossCutting/         CPF/CNPJ/CEP/phone helpers, clock
├── CadastroCliente.Testes/        Unit and integration tests
├── docs/                          Architecture, HTTP samples
└── docker-compose.yml
```

---

## Docker

```bash
docker compose up --build
curl http://localhost:8080/health/ready
```

The image runs as uid `10001`. Compose mounts a volume on `/app/data`, uses a read-only root filesystem and a tmpfs `/tmp`. Set a production `Jwt__Key` (32+ characters, not `dev-only`) before exposing the container.

---

## Tests

```bash
dotnet test CadastroClientes.sln -c Release --collect:"XPlat Code Coverage"
```

The suite covers domain invariants, application services, the SQLite repository and the full HTTP surface through `WebApplicationFactory`.

---

## Documentation

| Document | Description |
|----------|-------------|
| [docs/architecture.md](docs/architecture.md) | Layers, data flow, extension points |
| [docs/api.md](docs/api.md) | Status codes, filters, payloads |
| [docs/http/clientes.http](docs/http/clientes.http) | Copy-paste HTTP samples |
| [docs/code-review.md](docs/code-review.md) | Review of the original skeleton and what changed |

---

## License

MIT License. See [LICENSE](LICENSE).

---

## Author

[Edinaldosa2](https://github.com/Edinaldosa2)

CadastroClientes is a **customer master-data API**. It helps you register people and companies with Brazilian document rules — not a CRM, billing engine, or identity provider.

---

<a id="br-português"></a>

# Cadastro de Clientes API

**API REST pronta para uso de cadastro de clientes no Brasil — pessoas, empresas, endereços, contatos e status operacional.**

CadastroClientes é uma **Web API .NET 8** para **cadastro mestre com CPF/CNPJ válido**, **endereços e contatos**, **ativação / inativação / bloqueio**, **exclusão lógica**, **exportação CSV** e **health checks**. Roda localmente com SQLite — sem banco externo.

---

## O que há de novo na v1.2.0

| Área | Atualização |
|------|-------------|
| **Perfil admin** | O JWT `admin` inclui `admin` + `escrita` + `leitura` e gerencia a API inteira |
| **Gestão** | `GET /api/v1/admin/usuarios` e auditoria global paginada em `GET /api/v1/admin/auditoria` |
| **Reset** | `POST /api/v1/dev/reset` passa a ser exclusivo do admin |

## O que há de novo na v1.0.0

| Área | Atualização |
|------|-------------|
| **Superfície REST** | Rotas versionadas `/api/v1` para clientes, endereços, contatos, status e relatórios |
| **Documentos** | Dígitos verificadores de CPF e CNPJ, saída formatada, índice único |
| **Recursos aninhados** | Endereços (tipo, CEP, UF, principal) e contatos (e-mail, telefone, celular, WhatsApp) |
| **Ciclo de vida** | Ativar, inativar, bloquear, excluir logicamente e restaurar |
| **Auth** | JWT `admin` / `escrita` / `leitura` em `POST /api/v1/auth/token` |
| **Operação** | Swagger, health de arquivo, reset de demo, Docker sem root, POST idempotente persistido |
| **Testes** | Cobertura unitária e de integração (~95% das linhas) |

---

## Clonar e executar

```bash
git clone https://github.com/Edinaldosa2/CadastroClientesAPI.git
cd CadastroClientesAPI
dotnet run --project CadastroClientes --urls http://localhost:5105
```

Swagger: [http://localhost:5105/swagger](http://localhost:5105/swagger).

Com Docker: `docker compose up --build` → `http://localhost:8080`.

A primeira execução cria o SQLite e duas sementes de demonstração.

---

## Rotas

Consulte a tabela em [HTTP Routes](#http-routes). Autentique com `admin` / `admin-dev` (tudo), `editor` / `editor-dev` (escrita) ou `leitor` / `leitor-dev` (leitura). Os erros seguem Problem Details (RFC 7807) com `code`. Filtros: `nome`, `documento`, `contato`, `criadoDe`, `depoisDe`, `pagina`, `ordenarPor`.

---

## Requisitos

.NET 8 SDK. SQLite incluso. Docker opcional.

---

## Licença

MIT. Ver [LICENSE](LICENSE).

Autor: [Edinaldosa2](https://github.com/Edinaldosa2)
