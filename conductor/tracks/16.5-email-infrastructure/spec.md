# Track 16.5: Email Infrastructure Planning - Specification

## 1. Overview
Establish the core SMTP infrastructure for the platform to support automated communications. This includes defining the `IEmailSender` abstraction in the Application layer and providing an implementation using MailKit in the Infrastructure layer.

## 2. Technical Specifications

### 2.1 Interface: IEmailSender
The interface defines the contract for sending emails, allowing the Application layer to remain agnostic of the specific SMTP provider.

**Namespace**: `MeuSitePessoal.Application.Common.Interfaces`

```csharp
/// <summary>
/// Provides a mechanism for sending email messages.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message asynchronously to a single recipient.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The HTML or plain text body of the email.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
}
```

### 2.2 Configuration: EmailSettings
The settings model captures required SMTP parameters from the configuration stores.

**Namespace**: `MeuSitePessoal.Infrastructure.Configuration`

```csharp
/// <summary>
/// Represents the SMTP configuration settings required for email delivery.
/// </summary>
public record EmailSettings
{
    /// <summary>
    /// Gets the section name for binding configuration.
    /// </summary>
    public const string SectionName = "EmailSettings";

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string SenderEmail { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
}
```

### 2.3 Implementation: MailKitEmailService
The infrastructure implementation utilizes MailKit and MimeKit for robust email delivery.

**Namespace**: `MeuSitePessoal.Infrastructure.Services`
**File Path**: `MeuSitePessoal.Infrastructure/Services/MailKitEmailService.cs`

- **Library**: MailKit (Open source cross-platform .NET mail-client library).
- **Behavior**:
    - Constructs a `MimeMessage` using the provided parameters and `EmailSettings`.
    - Connects to the SMTP server using `SmtpClient.ConnectAsync`.
    - Authenticates using `SmtpClient.AuthenticateAsync`.
    - Sends the message using `SmtpClient.SendAsync`.
    - Disconnects gracefully.

### 2.4 Project Structure & Folders
- **Application Layer**:
    - `Interfaces/IEmailSender.cs`: The service contract.
- **Infrastructure Layer**:
    - `Configuration/EmailSettings.cs`: The configuration model.
    - `Services/MailKitEmailService.cs`: The MailKit implementation.
    - `Resources/EmailTemplates/`: (Reserved for future use) Folder for HTML templates or Liquid files.
- **Infrastructure Packages**:
    - `MailKit`
    - `MimeKit`

## 3. Dependency Injection
Registration will be handled via an extension method in the Infrastructure layer to keep `Program.cs` clean.

**Target File**: `MeuSitePessoal.Infrastructure/DependencyInjection.cs`

- The `EmailSettings` will be bound using `services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName))`.
- The `IEmailSender` will be registered as **Transient** to ensure a fresh client (or connection management) per send operation, unless connection pooling is later implemented.

## 4. Third-Person English Standards
- Code comments must use third-person singular (e.g., "The service connects to...", "The method validates...").
- Documentation should be concise and professional.

## 5. Definition of Done
- `IEmailSender` interface is defined in the Application layer.
- `MailKitEmailService` is implemented and registered in the Infrastructure layer.
- `EmailSettings` is correctly bound to `appsettings.json`.
- **Automated Unit Tests**: Verify that the `MailKitEmailService` correctly maps the `EmailSettings` and parameters to a `MimeMessage` object.
- **Automated Integration Tests**: Implement a test in `MeuSitePessoal.Tests.Integration` that performs a real SMTP delivery to Mailtrap to ensure the credentials and connection logic are valid.
- A manual integration test confirms successful delivery (e.g., via Mailtrap).

