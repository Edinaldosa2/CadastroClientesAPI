# API reference

Base URL (local): `http://localhost:5105`

Content type: `application/json` with string enums (`Fisica`, `Ativo`, `Residencial`, `Email`, …).

## Status codes

| Code | When |
|------|------|
| 200 | Read, replace, patch, status change |
| 201 | Create |
| 204 | Soft delete or nested delete |
| 400 | FluentValidation / malformed Idempotency-Key |
| 401 | API key configured and missing/wrong |
| 404 | Unknown customer, address or contact |
| 409 | Duplicate document or contact; idempotency key reused with another body; concurrency |
| 422 | Domain rule (invalid CPF, blocked customer, status already applied) |
| 429 | Rate limit |
| 500 | Unhandled (detail included outside Production) |

## List filters

`GET /api/v1/clientes`

| Query | Default | Notes |
|-------|---------|-------|
| nome | | Case-insensitive contains on nome/nomeFantasia |
| documento | | Digits only, contains |
| tipoPessoa | | `Fisica` or `Juridica` |
| status | | `Ativo`, `Inativo`, `Bloqueado` |
| cidade / uf | | Matches any address |
| incluirExcluidos | false | Include soft-deleted |
| pagina | 1 | |
| tamanhoPagina | 20 | Capped at 100 |
| ordenarPor | nome | `nome`, `documento`, `status`, `criadoEm`, `atualizadoEm` |
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
  "temAnterior": false
}
```

## Create customer

`POST /api/v1/clientes`

Optional header: `Idempotency-Key` (max 80 chars). Repeat with the same key and body to receive the original `201`.

Documento is stored as digits only and returned both raw and formatted.

## Patch

`PATCH /api/v1/clientes/{id}`

Omitted fields stay unchanged. To clear `observacoes` or `dataNascimento`, set the value to `null` and the matching `atualizar*` flag to `true`.

## Reports

- `GET /api/v1/relatorios/clientes/resumo` — totals by status, person type and UF of the principal address.
- `GET /api/v1/relatorios/clientes/exportar` — `text/csv` with `;` separator.
