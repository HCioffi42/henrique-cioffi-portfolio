using MeuSitePessoal.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuSitePessoal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokenService, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Hardcoded user for administrative operations
        if (request.Username == "admin" && request.Password == "admin123")
        {
            var token = _tokenService.GenerateToken(request.Username);
            _logger.LogInformation("Successful login for user: {Username}", request.Username);
            return Ok(new { Token = token });
        }

        _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
        return Unauthorized("Invalid credentials.");
    }

    public record LoginRequest(string Username, string Password);
}
