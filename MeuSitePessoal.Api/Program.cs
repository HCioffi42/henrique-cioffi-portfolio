using System.Text;
using AspNet.Security.OAuth.GitHub;
using FluentValidation;
using MeuSitePessoal.Api.Middleware;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Common.Behaviors;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure;
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

// HC: Visual log to confirm environment type during startup
Console.WriteLine(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
    ? "Running on Docker!"
    : "Running locally!");

// Add custom logging
builder.Services.AddCustomLogging(builder.Configuration);
builder.Services.AddCustomTracing();
builder.Host.UseSerilog();

// Register custom infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Register Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedEmail = true;
})

.AddEntityFrameworkStores<BlogDbContext>()
.AddDefaultTokenProviders();

// Register Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IStorageService, LocalStorageService>();
builder.Services.AddSingleton<IFeatureToggleService, FeatureToggleService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

var authBuilder = builder.Services.AddAuthentication(options =>
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

// Conditionally registers Google OAuth if credentials are configured.
var googleClientId = builder.Configuration["OAuthSettings:Google:ClientId"];
var googleClientSecret = builder.Configuration["OAuthSettings:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

// Conditionally registers GitHub OAuth if credentials are configured.
var githubClientId = builder.Configuration["OAuthSettings:GitHub:ClientId"];
var githubClientSecret = builder.Configuration["OAuthSettings:GitHub:ClientSecret"];
if (!string.IsNullOrWhiteSpace(githubClientId) && !string.IsNullOrWhiteSpace(githubClientSecret))
{
    authBuilder.AddGitHub(options =>
    {
        options.ClientId = githubClientId;
        options.ClientSecret = githubClientSecret;
    });
}

builder.Services.AddAuthorization();

// Registers AutoMapper to handle mapping between DTOs/Commands and Entities.
builder.Services.AddAutoMapper(typeof(CreateArticleCommand).Assembly);

// Registers MediatR to manage Commands and Handlers from the Application layer.
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateArticleCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Registers all the validators defined in the Application layer.
builder.Services.AddValidatorsFromAssembly(typeof(CreateArticleCommand).Assembly);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BlogDbContext>("PostgreSQL");

// Add Memory Cache
builder.Services.AddMemoryCache();

// Adds support for ASP.NET Core Controllers, allowing route organization in separate classes.
builder.Services.AddControllers();

// Configures Swagger for API documentation with JWT support.
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
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enables the global exception handling middleware at the beginning of the pipeline.
app.UseExceptionHandler();

app.UseCors("DefaultPolicy");
app.UseStaticFiles();

// Enable Serilog request logging
app.UseSerilogRequestLogging();

// Enables Swagger only in the development environment to facilitate API testing.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Maps the routes defined in the Controllers so the application can respond to requests.
app.MapControllers();

// Maps the health check endpoint to /health.
app.MapHealthChecks("/health");

// Executes the data seed asynchronously during startup, skipping it if running in the testing environment.
// HC: Robust database migration with retry logic to wait for Postgres startup
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BlogDbContext>();
    
    int retryCount = 10; 
    while (retryCount > 0)
    {
        try
        {
            Console.WriteLine("--> Trying to apply migrations... (Attempt " + (11 - retryCount) + ")");
            await context.Database.MigrateAsync();
            Console.WriteLine("--> Migrations applied successfully!");
            break;
        }
        catch (Exception)
        {
            retryCount--;
            Console.WriteLine($"--> Database is not ready yet. Waiting 3s...");
            if (retryCount == 0) throw;
            await Task.Delay(3000);
        }
    }
    
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    Console.WriteLine("--> Starting data seed...");
    await DbInitializer.SeedAsync(context, userManager, roleManager, configuration, logger);
    Console.WriteLine("--> Initialization cycle FINISHED, Boss!");
}

app.Run();

// Expose the Program class to the testing project
namespace MeuSitePessoal.Api  { public partial class Program { } }