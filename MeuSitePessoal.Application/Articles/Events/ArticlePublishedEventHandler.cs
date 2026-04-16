using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Events;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Application.Articles.Events;

/// <summary>
/// Handles the ArticlePublishedEvent by sending a notification to all active newsletter subscribers.
/// Uses parallel processing to improve performance and avoids hardcoded environment URLs.
/// </summary>
public class ArticlePublishedEventHandler : INotificationHandler<ArticlePublishedEvent>
{
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<ArticlePublishedEventHandler> _logger;
    private readonly IConfiguration _configuration;

    public ArticlePublishedEventHandler(
        ISubscriberRepository subscriberRepository,
        IEmailSender emailSender,
        IEmailTemplateService templateService,
        ILogger<ArticlePublishedEventHandler> logger,
        IConfiguration configuration)
    {
        _subscriberRepository = subscriberRepository;
        _emailSender = emailSender;
        _templateService = templateService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Handle(ArticlePublishedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing ArticlePublishedEvent for Article: {ArticleId} - {ArticleTitle}", 
            notification.Article.Id, notification.Article.Title);

        try
        {
            var subscribers = await _subscriberRepository.GetActiveSubscribersAsync();
            
            if (!subscribers.Any())
            {
                _logger.LogInformation("No active subscribers found. Skipping email notifications.");
                return;
            }

            // Retrieves separate URLs for the Frontend (Client) and Backend (API).
            // This prevents port mismatches during local development and ensures absolute URLs in production.
            var clientBaseUrl = _configuration["ClientSettings:BaseUrl"] ?? "https://hcioffi.dev";
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://hcioffi.dev";
            
            // Limits the number of concurrent email tasks to avoid overwhelming the SMTP provider or local resources.
            var parallelOptions = new ParallelOptions 
            { 
                MaxDegreeOfParallelism = 5,
                CancellationToken = cancellationToken 
            };

            int successCount = 0;
            int failureCount = 0;

            // Processes the subscriber list in parallel using modern .NET asynchronous loops.
            await Parallel.ForEachAsync(subscribers, parallelOptions, async (subscriber, ct) =>
            {
                try
                {
                    // The unsubscribe link must point to the API Base URL so the controller can process the request.
                    var unsubscribeUrl = $"{apiBaseUrl}/api/newsletter/unsubscribe?email={Uri.EscapeDataString(subscriber.Email)}&token={subscriber.UnsubscribeToken}";
                    
                    // The article link points to the Client Base URL where the reader consumes the content.
                    var articleUrl = $"{clientBaseUrl}/article/{notification.Article.Id}";
                    
                    // Renders the HTML content for the specific article notification.
                    var htmlBody = await _templateService.RenderTemplateAsync("Email/NewPostNotification", (notification.Article, articleUrl, unsubscribeUrl));
                    
                    // Dispatches the email through the configured SMTP provider (e.g., Resend).
                    await _emailSender.SendEmailAsync(
                        subscriber.Email, 
                        $"New Post: {notification.Article.Title} - hcioffi.dev", 
                        htmlBody,
                        ct);
                    
                    Interlocked.Increment(ref successCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send newsletter email to {Email}", subscriber.Email);
                    Interlocked.Increment(ref failureCount);
                }
            });

            _logger.LogInformation("Newsletter batch completed. Success: {SuccessCount}, Failure: {FailureCount}", 
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling ArticlePublishedEvent for Article: {ArticleId}", 
                notification.Article.Id);
        }
    }
}