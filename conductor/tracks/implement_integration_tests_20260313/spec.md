# Specification: Implementar testes de integração em ArtigosIntegrationTests.cs com Testcontainers

## Goal
Implementar testes de integração robustos para a API de Artigos, utilizando o `BaseIntegrationTest` para gerenciar um banco de dados PostgreSQL real via Testcontainers.

## Scope
- Refatorar `BaseIntegrationTest.cs` para suportar `WebApplicationFactory` e injeção do banco real.
- Refatorar `ArtigosIntegrationTests.cs` para herdar de `BaseIntegrationTest`.
- Implementar testes para:
    - Criar um novo artigo (POST).
    - Obter um artigo por ID (GET).
    - Listar todos os artigos (GET).
- Garantir que as migrações sejam executadas no banco do container antes dos testes.

## Constraints
- Usar `postgres:15-alpine` como imagem do banco.
- Utilizar `WebApplicationFactory<Program>` para hospedar a API em memória.
- Substituir o `BlogDbContext` padrão para apontar para a connection string do container.

## Implementation Details
- `BaseIntegrationTest` deve gerenciar a `WebApplicationFactory`.
- O método `ConfigureWebHost` da factory deve ser sobrescrito para reconfigurar o banco.
- Usar `respawn` ou recriação do banco se necessário, mas para este escopo inicial, a recriação/migração por teste ou por fixture é aceitável.
