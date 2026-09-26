# Backlog

Quick wins ainda não implementados. Cada item cabe num commit.

## Identidade e admin

1. Persistência de usuários no SQLite (tabela `usuarios`) em vez de `Users` no `appsettings`.
2. Hash de senha com PBKDF2/BCrypt; parar de comparar texto plano.
3. `POST /api/v1/admin/usuarios` para criar conta (`usuario`, `perfil`, senha).
4. `PATCH /api/v1/admin/usuarios/{usuario}` para trocar perfil (`leitura` / `escrita` / `admin`).
5. `POST /api/v1/admin/usuarios/{usuario}/bloquear` e `/desbloquear`.
6. `POST /api/v1/admin/usuarios/{usuario}/senha` (admin redefine) + `POST /api/v1/auth/senha` (próprio usuário).
7. Impedir o último `admin` de se rebaixar ou se bloquear.
8. Seed de usuários só em Development; Production exige `Users` via env/secrets.
9. `GET /api/v1/auth/me` devolvendo `usuario`, `perfil` e `roles`.
10. Expiração curta do JWT + `refreshToken` persistido (revogável pelo admin).

## Consulta, admin e DX da API

11. Filtros na auditoria global: `usuario`, `tipo`, `clienteId`, `de`, `ate`.
12. `GET /api/v1/admin/auditoria/exportar` (CSV com BOM, só admin).
13. `HEAD /api/v1/clientes/{id}` devolvendo só `ETag` e `X-Total-Count` onde couber.
14. `OPTIONS` explícito no catálogo com métodos permitidos por recurso.
15. Campo `q` na listagem (nome + documento + contato numa só query).
16. Filtro `temEndereco` / `temContato` / `semContato` na listagem.
17. `GET /api/v1/clientes/{id}/timeline` unificando status + auditoria do cliente.
18. `If-None-Match` no GET por id → `304 Not Modified`.
19. Header `Link: <...>; rel="next"` além de `X-Next-Cursor`.
20. `POST /api/v1/clientes/validar` (dry-run do FluentValidation sem persistir).

## Domínio e consistência

21. Eventos de domínio em endereço/contato (hoje só o cliente emite).
22. Motivo obrigatório também em `inativar` (hoje só no desbloqueio).
23. Limite configurável de endereços/contatos (`Limites:EnderecosPorCliente`) em vez de 10/15 fixos.
24. `NomeSocial` (PF) e `InscricaoMunicipal` (PJ) no agregado.
25. Validar IE por UF quando o cliente for jurídico.
26. Impedir segundo endereço principal no mesmo `POST` de criação.
27. TTL na tabela `idempotency_keys` (job ou limpeza no reset/admin).
28. `409` estável `documento_duplicado` no create (hoje `conflict` genérico).

## Operação e segurança

29. `appsettings.Production.json` sem usuários `*-dev` e com `AllowedHosts` restrito.
30. Recusar start em Production se existir senha `*-dev` ou `Users` vazio.
31. Rate limit por usuário JWT (hoje só por IP).
32. `Retry-After` no `429`.
33. Health `/health/ready` falhar se o volume SQLite estiver read-only de verdade.
34. Primeira migration EF (`InitialCreate`) no lugar de só `EnsureCreated`.
35. Job no CI: `docker build` da imagem (hoje só `dotnet test`).
36. Serilog JSON com `traceId` + documento mascarado.

## Testes e docs

37. Teste de host `Production` cobrindo reset `403` e JWT `dev-only`.
38. Teste de contrato: OpenAPI exige `Bearer` nas rotas protegidas.
39. `docs/http/admin.http` com login admin, usuários, auditoria e reset.
40. Trocar a default do GitHub para `main` (Settings → Default branch). `main` e `master` já estão no mesmo SHA.

Os itens 1–10 fecham a gestão de contas como admin. Os 11–20 melhoram o uso diário da API. Os 29–36 preparam um piloto exposto na internet.
