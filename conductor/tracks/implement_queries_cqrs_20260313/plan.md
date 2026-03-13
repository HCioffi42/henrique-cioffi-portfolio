# Plan: Implementar as Queries (ListarTodos e ObterPorId) seguindo o padrão CQRS com MediatR

## Objective
Implementar as queries para listar todos os artigos e obter um artigo por ID utilizando CQRS e MediatR.

## Key Files & Context
- `MeuSitePessoal.Application/Queries/`: Novo diretório para as queries.
- `MeuSitePessoal.Application/Handlers/`: Local onde os handlers serão criados.
- `MeuSitePessoal.Api/Controllers/ArtigosController.cs`: Controller a ser refatorado.
- `MeuSitePessoal.Domain/Interfaces/IArtigoRepository.cs`: Interface do repositório a ser utilizada.
- `MeuSitePessoal.Tests/`: Local onde os testes serão adicionados.

## Implementation Steps

### Phase 1: Preparação
- [x] Criar diretório `MeuSitePessoal.Application/Queries`.

### Phase 2: Queries e Handlers
- [x] Criar `GetTodosArtigosQuery` em `MeuSitePessoal.Application/Queries/GetTodosArtigosQuery.cs`.
- [x] Criar `GetTodosArtigosHandler` em `MeuSitePessoal.Application/Handlers/GetTodosArtigosHandler.cs`.
- [x] Criar `GetArtigoByIdQuery` em `MeuSitePessoal.Application/Queries/GetArtigoByIdQuery.cs`.
- [x] Criar `GetArtigoByIdHandler` in `MeuSitePessoal.Application/Handlers/GetArtigoByIdHandler.cs`.

### Phase 3: Refatoração da API
- [x] Modificar `ArtigosController.cs` para utilizar `IMediator` nos métodos `ListarTodos` e `ObterPorId`.

### Phase 4: Testes
- [x] Criar testes unitários para `GetTodosArtigosHandler`.
- [x] Criar testes unitários para `GetArtigoByIdHandler`.

## Verification & Testing
- [x] Executar os novos testes unitários.
- [x] Validar via requisição HTTP (`GET /api/artigos` e `GET /api/artigos/{id}`) que as listagens continuam funcionando corretamente (verificado via testes de integração).
