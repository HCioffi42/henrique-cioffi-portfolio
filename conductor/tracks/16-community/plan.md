# Anonymous Newsletter System Implementation Plan

## Step 1: Domain Setup
1.  **Create Entity:** `MeuSitePessoal.Domain/Entities/Subscriber.cs`
    *   Properties: `Id` (Guid), `Email` (string), `SubscribedAt` (DateTime), `IsActive` (bool).
    *   Include XML documentation in the third person.

## Step 2: Infrastructure & Database Migration
1.  **Update DbContext:** `MeuSitePessoal.Infrastructure/Data/BlogDbContext.cs`
    *   Add `public DbSet<Subscriber> Subscribers { get; set; }`
    *   In `OnModelCreating`, configure the `Email` field to be unique: `builder.Entity<Subscriber>().HasIndex(s => s.Email).IsUnique();`
2.  **Generate Migration:**
    *   Using the .NET CLI or Package Manager Console, run `dotnet ef migrations add AddSubscriberEntity` within the Infrastructure directory or from the root pointing to context.
    *   *(Note: The migration script must be executed to update PostgreSQL before running the app).*

## Step 3: Application Layer (CQRS & Validation)
1.  **Create Command:** `MeuSitePessoal.Application/Newsletter/Commands/Subscribe/SubscribeToNewsletterCommand.cs`
    *   Record taking `string Email`. It will return a custom object like `SubscribeResult` specifying success or specific error codes (e.g., AlreadySubscribed).
2.  **Create Validator:** `SubscribeToNewsletterCommandValidator.cs`
    *   Use FluentValidation `RuleFor(x => x.Email).NotEmpty().EmailAddress()`.
3.  **Create Handler:** `SubscribeToNewsletterCommandHandler.cs`
    *   Inject `BlogDbContext`.
    *   Check for existing subscriber by email.
    *   If found & `IsActive`: Return "Already Subscribed" conflict result.
    *   If found & not `IsActive`: Update `IsActive = true`, save changes, returning success.
    *   If not found: Create `new Subscriber`, add to database, save changes, returning success.

## Step 4: API Presentation Layer
1.  **Create Controller:** `MeuSitePessoal.Api/Controllers/NewsletterController.cs`
    *   Add `[HttpPost("subscribe")]` and `[AllowAnonymous]`.
    *   Send the command via `IMediator`.
    *   Map the handler result to HTTP status codes: `Ok()` for success, `Conflict(new { message = "..." })` if already subscribed.

## Step 5: Frontend Development (meupessoal-web)
1.  **Create API Service Function:** Inside an appropriate service file (e.g., `services/newsletterService.ts` or similar based on existing files), define `subscribeToNewsletter(email: string)`.
2.  **Create Component:** `src/components/NewsletterBox.tsx` (or `NewsletterBox/index.tsx`).
    *   Render a simple form with one `input[type="email"]` and a submit button.
    *   Keep local state for `email` string, `status` (loading, success, error), and `errorMessage`.
    *   Handle `409` explicit response setting exact error string provided.
3.  **Integrate Component:** Add `<NewsletterBox />` to a persistent layout spot like `Footer.tsx` or `App.tsx` sidebar.
