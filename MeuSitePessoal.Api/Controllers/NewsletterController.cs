using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Application.Newsletter.Commands.ConfirmSubscription;
using MeuSitePessoal.Application.Newsletter.Commands.Subscribe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuSitePessoal.Api.Controllers;

/// <summary>
/// Manages HTTP requests related to the newsletter system.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the controller with the MediatR instance.
    /// </summary>
    public NewsletterController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Subscribes an email to the newsletter, initiating a double opt-in flow.
    /// </summary>
    [HttpPost("subscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeToNewsletterCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(new { message = "Subscription initiated. Please check your email to confirm." });
        }

        if (result.Type == ErrorType.Conflict)
        {
            return Conflict(new { message = result.Error });
        }

        return BadRequest(new { message = result.Error });
    }

    /// <summary>
    /// Confirms a newsletter subscription using the provided email and token.
    /// </summary>
    [HttpGet("confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> Confirm([FromQuery] string email, [FromQuery] string token)
    {
        var command = new ConfirmSubscriptionCommand(email, token);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(new { message = "Subscription confirmed successfully! Welcome to the newsletter." });
        }

        if (result.Type == ErrorType.NotFound)
        {
            return NotFound(new { message = result.Error });
        }

        return BadRequest(new { message = result.Error });
    }
}

