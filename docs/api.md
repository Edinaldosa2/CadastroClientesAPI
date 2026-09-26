# API reference

Base URL (local): `http://localhost:5105`

Content type: `application/json` with string enums (`Fisica`, `Ativo`, `Residencial`, `Email`, …).

Protected routes require `Authorization: Bearer {token}` from `POST /api/v1/auth/token`. Demo users: `editor` / `editor-dev` (escrita) and `leitor` / `leitor-dev` (leitura).

## Status codes

| Code | When |
|------|------|
| 200 | Read, replace, patch, status change |
| 201 | Create |
| 204 | Soft delete or nested delete |
| 400 | FluentValidation / malformed Idempotency-Key / client-supplied id |
| 401 | Missing JWT, invalid credentials, or API key configured and missing/wrong |
| 403 | Role `leitura` on a write, CSV export, `incluirExcluidos`, or reset |
| 404 | Unknown customer, address or contact |
| 405 | `DELETE /api/v1/clientes` (collection delete is blocked) |
| 409 | Duplicate document or contact; idempotency key reused with another body; restore conflict; concurrency |
| 412 | `If-Match` does not match the current ETag (`etag_conflito`) |
| 422 | Domain rule (invalid CPF, blocked customer, immutable document, unlock without reason) |
| 429 | Rate limit |
| 500 | Unhandled (stack detail only outside Production) |

Problem Details include a stable `code` (`cpf_invalido`, `documento_imutavel`, `cliente_bloqueado`, `motivo_obrigatorio`, `id_invalido`, `etag_conflito`, `idempotency_conflito`, …).

## List filters

`GET /api/v1/clientes`

| Query | Default | Notes |
|-------|---------|-------|
| nome | | Case-insensitive contains on nome/nomeFantasia |
| documento | | Digits only, contains |
| tipoPessoa | | `Fisica` or `Juridica` |
| status | | `Ativo`, `Inativo`, `Bloqueado` |
| cidade / uf | | Matches any address |
| contato | | Matches contact value (email or digits) |
| criadoDe / criadoAte | | UTC creation window |
| depoisDe | | Cursor: customer id; response may include `X-Next-Cursor` |
| incluirExcluidos | false | Soft-deleted rows; requires role `escrita` |
| pagina | 1 | Ignored when `depoisDe` is set |
| tamanhoPagina | 20 | Capped at 100 |
| ordenarPor | nome | Whitelist: `nome`, `documento`, `status`, `criadoEm`, `atualizadoEm` |
| descendente | false | |

Response envelope:

```json
{
  "itens": [],
  "pagina": 1,
  "tamanhoPagina": 20,
  "total": 2,
  "totalPaginas": 1,
  "temProxima": false,
  "temAnterior": false,
  "proximoCursor": null
}
```

`GET /api/v1/clientes/{id}` returns `ETag`. Send `If-Match` on PUT/PATCH to reject stale writes.

## Create customer

`POST /api/v1/clientes` — role `escrita`.

Optional header: `Idempotency-Key` (max 80 chars). The key and body hash are persisted. Repeat with the same key and body to receive the original `201`. A different body with the same key returns `409` (`idempotency_conflito`). Client-supplied `id` is rejected.

Documento is stored as digits only and is immutable after create.

## Patch

`PATCH /api/v1/clientes/{id}`

Omitted fields stay unchanged. To clear `observacoes` or `dataNascimento`, set the value to `null` and the matching `atualizar*` flag to `true`. Sending `documento` (property or extra field) returns `422` (`documento_imutavel`).

## Status

Unlocking a blocked customer requires `{ "motivo": "..." }`. Soft delete is logical only; `ClienteRepository.Remove` throws. Restore is blocked when another active customer already owns the document.

## Reports

- `GET /api/v1/relatorios/clientes/resumo` — totals by status, person type and UF of the principal address (leitura).
- `GET /api/v1/relatorios/clientes/exportar` — UTF-8 BOM `text/csv` with `;` separator (escrita).

## Operations

- `POST /api/v1/dev/reset` — wipe and reseed. Role `escrita`. `403` in Production.
- `GET /health`, `/health/live`, `/health/ready` — anonymous. Ready includes SQLite file check.
- Swagger is anonymous outside Production; Production requires a valid JWT.
