using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using MeuSitePessoal.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using MeuSitePessoal.Application.Commands;

var builder = WebApplication.CreateBuilder(args);

// Configura o DbContext para utilizar o PostgreSQL com a connection string definida no appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registra o repositório utilizando o ciclo de vida Scoped, o que garante que uma nova instância seja criada para cada requisição HTTP, mantendo a consistência com o DbContext.
builder.Services.AddScoped<IArtigoRepository, ArtigoRepository>();

// Registra o MediatR para gerenciar os Commands e Handlers da camada de Application.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateArtigoCommand).Assembly));

// Adiciona o suporte aos Controllers do ASP.NET Core, permitindo a organização das rotas em classes separadas.
builder.Services.AddControllers();

// Configura o Swagger para documentação da API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Habilita o Swagger apenas no ambiente de desenvolvimento para facilitar os testes da API.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mapeia as rotas definidas nos Controllers para que a aplicação possa responder às requisições.
app.MapControllers();

app.Run();