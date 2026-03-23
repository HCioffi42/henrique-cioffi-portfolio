using System.Text;
using FluentValidation;
using MeuSitePessoal.Api.Middleware;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Common.Behaviors;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Configuration;
using MeuSitePessoal.Infrastructure.Data;
using MeuSitePessoal.Infrastructure.Logging;
using MeuSitePessoal.Infrastructure.Repositories;
using MeuSitePessoal.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add custom logging
builder.Services.AddCustomLogging(builder.Configuration);
builder.Host.UseSerilog();

// Configura o DbContext para utilizar o PostgreSQL com a connection string definida no appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<BlogDbContext>()
.AddDefaultTokenProviders();

// Register Services
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IStorageService, LocalStorageService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// Registers AutoMapper to handle mapping between DTOs/Commands and Entities.
builder.Services.AddAutoMapper(typeof(CreateArticleCommand).Assembly);

// Registra o MediatR para gerenciar os Commands e Handlers da camada de Application.
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateArticleCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Registra todos os validators definidos na camada de Application.
builder.Services.AddValidatorsFromAssembly(typeof(CreateArticleCommand).Assembly);

// Adiciona o suporte aos Controllers do ASP.NET Core, permitindo a organização das rotas em classes separadas.
builder.Services.AddControllers();

// Configura o Swagger para documentação da API com suporte a JWT.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MeuSitePessoal API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Registers the global exception handler and problem details services.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebAppPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite/React
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

app.UseCors("WebAppPolicy");

// Enables serving static files (such as images in wwwroot).
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Mapeia as rotas definidas nos Controllers para que a aplicação possa responder às requisições.
app.MapControllers();

// Executes the data seed asynchronously during startup, skipping it if running in the testing environment.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BlogDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
    await context.Database.MigrateAsync(); 
        
    await DbInitializer.SeedAsync(context, userManager, roleManager);
}

app.Run();

// Expose the Program class to the testing project
namespace MeuSitePessoal.Api  { public partial class Program { } }
