using FluentValidation;
using MeuSitePessoal.Api.Middleware;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using MeuSitePessoal.Application.Common.Behaviors;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using MeuSitePessoal.Infrastructure.Logging;
using MeuSitePessoal.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add custom logging
builder.Services.AddCustomLogging(builder.Configuration);
builder.Host.UseSerilog();

// Configura o DbContext para utilizar o PostgreSQL com a connection string definida no appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registra o repositório utilizando o ciclo de vida Scoped, o que garante que uma nova instância seja criada para cada requisição HTTP, mantendo a consistência com o DbContext.
builder.Services.AddScoped<IArtigoRepository, ArtigoRepository>();

// Registra o MediatR para gerenciar os Commands e Handlers da camada de Application.
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateArtigoCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Registra todos os validators definidos na camada de Application.
builder.Services.AddValidatorsFromAssembly(typeof(CreateArtigoCommand).Assembly);

// Adiciona o suporte aos Controllers do ASP.NET Core, permitindo a organização das rotas em classes separadas.
builder.Services.AddControllers();

// Configura o Swagger para documentação da API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registers the global exception handler and problem details services.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Enables the global exception handling middleware at the beginning of the pipeline.
app.UseExceptionHandler();

// Enable Serilog request logging
app.UseSerilogRequestLogging();

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

// Expose the Program class to the testing project
namespace MeuSitePessoal.Api  { public partial class Program { } }