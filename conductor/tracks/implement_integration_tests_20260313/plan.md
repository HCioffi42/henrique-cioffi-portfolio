# Plan: Implementar testes de integração em ArtigosIntegrationTests.cs com Testcontainers

## Objective
Configurar e implementar testes de integração utilizando Testcontainers para validar o fluxo completo da API com banco real.

## Key Files & Context
- `MeuSitePessoal.Tests/Integration/BaseIntegrationTest.cs`: Classe base a ser aprimorada.
- `MeuSitePessoal.Tests/Integration/ArtigosIntegrationTests.cs`: Classe de testes a ser refatorada.
- `MeuSitePessoal.Infrastructure/Data/BlogDbContext.cs`: Contexto do banco.

## Implementation Steps

### Phase 1: Aprimoramento da Base
- [x] Refatorar `BaseIntegrationTest.cs`:
    - Integrar com `WebApplicationFactory<Program>`.
    - Configurar o container PostgreSQL.
    - Sobrescrever `ConfigureWebHost` para usar a connection string do container.
    - Garantir que `DbContext` execute as migrações no startup do teste.

### Phase 2: Refatoração de ArtigosIntegrationTests
- [x] Modificar `ArtigosIntegrationTests.cs` para herdar de `BaseIntegrationTest`.
- [x] Remover a implementação manual de `IClassFixture<WebApplicationFactory<Program>>`.
- [x] Ajustar o construtor para receber a factory da base (se necessário).

### Phase 3: Implementação dos Casos de Teste
- [x] Teste: `CriarEObterArtigo_DeveFuncionarComBancoReal`:
    - Enviar POST.
    - Validar 201 Created.
    - Enviar GET com o ID retornado.
    - Validar dados no banco.
- [x] Teste: `ListarArtigos_DeveRetornarTodosOsArtigosCadastrados`:
    - Cadastrar múltiplos artigos.
    - Enviar GET para listar todos.
    - Validar contagem e dados.

### Phase 4: Validação
- [x] Executar `dotnet test` e garantir que o container suba, os testes passem e o container desça corretamente.
