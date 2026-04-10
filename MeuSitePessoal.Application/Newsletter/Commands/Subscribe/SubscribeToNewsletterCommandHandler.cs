using System.Text;
using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MeuSitePessoal.Application.Newsletter.Commands.Subscribe;

/// <summary>
/// Handles the execution of the <see cref="SubscribeToNewsletterCommand"/> by initiating a double opt-in verification flow.
/// </summary>
public class SubscribeToNewsletterCommandHandler : IRequestHandler<SubscribeToNewsletterCommand, Result>
{
    private readonly IBlogDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateService _templateService;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the handler with the necessary services.
    /// </summary>
    public SubscribeToNewsletterCommandHandler(
        IBlogDbContext dbContext,
        IEmailSender emailSender,
        IEmailTemplateService templateService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _templateService = templateService;
        _configuration = configuration;
    }

    /// <summary>
    /// Processes the subscription request, generating a verification token and sending an opt-in email.
    /// </summary>
    public async Task<Result> Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLower();
        var existingSubscriber = await _dbContext.Subscribers
            .FirstOrDefaultAsync(s => s.Email == normalizedEmail, cancellationToken);

        string token;
        
        if (existingSubscriber != null)
        {
            // HC: If the user is already verified and active, we inform them gracefully.
            if (existingSubscriber.IsVerified && existingSubscriber.IsActive)
                return Result.Success();

            // HC: If the user is not verified, we refresh the token and resend the email.
            token = GenerateToken();
            existingSubscriber.VerificationToken = token;
            existingSubscriber.IsActive = false; // Ensure they are inactive until verified
            
            _dbContext.Subscribers.Update(existingSubscriber);
        }
        else
        {
            // HC: New subscriber creation flow.
            token = GenerateToken();
            existingSubscriber = new Subscriber
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                SubscribedAt = DateTime.UtcNow,
                IsActive = false,
                IsVerified = false,
                VerificationToken = token
            };
            
            _dbContext.Subscribers.Add(existingSubscriber);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await SendVerificationEmail(normalizedEmail, token, cancellationToken);
        
        return Result.Success();
    }

    /// <summary>
    /// Generates a unique, URL-safe verification token.
    /// </summary>
    private static string GenerateToken()
    {
        var guid = Guid.NewGuid().ToString();
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(guid));
    }

    /// <summary>
    /// Constructs and sends the double opt-in verification email.
    /// </summary>
    private async Task SendVerificationEmail(string email, string token, CancellationToken ct)
    {
        var baseUrl = _configuration["ClientSettings:BaseUrl"] ?? "https://hcioffi.dev";
        var verificationUrl = $"{baseUrl}/newsletter/confirm?email={email}&token={token}";

        var subject = "Confirm your subscription to Meu Site Pessoal Newsletter";
        var body = await _templateService.RenderTemplateAsync("NewsletterVerification", new 
        { 
            ConfirmLink = verificationUrl 
        });

        await _emailSender.SendEmailAsync(email, subject, body, ct);
    }
}


