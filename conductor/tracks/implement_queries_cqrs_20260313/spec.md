# Specification: Implementar as Queries (ListarTodos e ObterPorId) seguindo o padrão CQRS com MediatR

## Goal
O objetivo desta track é refatorar a camada de API para utilizar o padrão CQRS (Command Query Responsibility Segregation) para as operações de leitura de artigos (Listar todos e Obter por ID), utilizando a biblioteca MediatR.

## Scope
- Criação das Queries `GetTodosArtigosQuery` e `GetArtigoByIdQuery`.
- Criação dos Handlers `GetTodosArtigosHandler` e `GetArtigoByIdHandler`.
- Refatoração do `ArtigosController` para delegar as consultas ao MediatR.
- Adição de testes unitários para os novos Handlers.

## Constraints
- Manter a consistência com o padrão já estabelecido para os Commands.
- O repositório `IArtigoRepository` deve continuar sendo utilizado nos Handlers.

## Implementation Details
- As Queries devem ser implementadas como `record` no namespace `MeuSitePessoal.Application.Queries`.
- Os Handlers devem ser implementados no namespace `MeuSitePessoal.Application.Handlers`.
- O `ArtigosController` deve injetar `IMediator` e utilizá-lo para enviar as Queries.
