# hcioffi.dev - Professional Blog & CMS

**Live Project**: [https://hcioffi.dev](https://hcioffi.dev)

This repository contains a full-stack personal blog system built with a Clean Architecture approach using .NET 8 for the backend and React for the frontend. It serves as my personal portfolio and a laboratory for software engineering best practices.

## 1. Tech Stack

  * **Backend**: .NET 8, ASP.NET Core Identity, Entity Framework Core, MediatR, JWT Authentication.
  * **Frontend**: React (Vite), TypeScript, Tailwind CSS, Axios, Context API.
  * **Database**: PostgreSQL (Production/Dev/CI).
  * **Infrastructure & CI**: GitHub Actions, Docker, Nginx (Reverse Proxy), Cloudflare.

-----

## 2. Developer Experience (DevEx) & Setup

To streamline the local environment setup, this project includes PowerShell automation scripts that handle environment variables and secret initialization.

### 2.1 Automated Setup (Recommended)

Run the following script to initialize environment variables and local secrets:

```powershell
./scripts/setup-local-env.ps1
```

### 2.2 Manual Backend Setup

1.  Navigate to the `MeuSitePessoal.Api` directory.
2.  **Initialize Local Secrets**: Configure your connection string:
    ```bash
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=MeuSiteDb;Username=postgres;Password=YOUR_PASSWORD"
    ```
    > **Rider Tip**: The secrets JSON can be edited directly by right-clicking the API project and selecting **Manage \> User Secrets**.
3.  Apply migrations: `dotnet ef database update`.
4.  Run the project: `dotnet run`.

-----

## 3. Continuous Integration & Quality (SDET Focus)

This project features a robust CI pipeline via **GitHub Actions** that enforces strict quality standards on every push or Pull Request.

  * **Automated Testing**:
      * Executes **88 automated tests** (Unit and Integration).
      * Integration tests utilize **PostgreSQL Service Containers** to ensure validation against a real database instance.
  * **Governance & Policies**:
      * **Branch Protection**: Direct pushes to `main` and `dev` are restricted via GitHub Rulesets.
      * **Automated Naming Validation**: A custom GitHub Action ensures all external contributions follow the required naming convention.
  * **Linting & Build**:
      * Strict ESLint checks for the frontend and production build validation for both stacks.

-----

## 4. Contributing

Contributions are what make the professional community an amazing place to learn and create. To maintain the project's integrity, please follow these guidelines:

1.  **Branch Naming**: External branches must start with the `contribution/` prefix (e.g., `contribution/JohnDoe-FixSearch`).
2.  **Pull Requests**: All changes must be submitted via PR. The merge is only allowed after all **88 tests** and naming policies pass.
3.  **Owner Bypass**: The repository owner maintains a bypass policy for rapid iterations (e.g., `feature/*` or `fix/*` branches).

-----

## 5. Security & Architecture

  * **Secret Management**: Zero sensitive data stored in source control.
  * **Clean Architecture**: Decoupled layers ensuring clear separation of concerns.
  * **Sequential Testing**: Integration tests are configured to run sequentially to prevent database race conditions.

-----

## 6. Project Roadmap

The detailed evolution of this project, including future tracks like Playwright E2E testing and Advanced Search, can be found in my **[Detailed Roadmap](https://github.com/HCioffi42/henrique-cioffi-portfolio/blob/dev/docs/roadmap.md)**.
