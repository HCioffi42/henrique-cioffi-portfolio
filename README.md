# hcioffi.dev - Professional Blog & CMS

![Backend Validation](https://github.com/HCioffi42/henrique-cioffi-portfolio/actions/workflows/ci.yml/badge.svg)
![Production Deployment](https://github.com/HCioffi42/henrique-cioffi-portfolio/actions/workflows/deploy.yml/badge.svg)

**Live Project**: [https://hcioffi.dev](https://hcioffi.dev)

This repository contains a full-stack personal blog system built with a Clean Architecture approach using .NET 8 for the backend and React for the frontend. It serves as my personal portfolio and a laboratory for software engineering best practices.

## 1. Tech Stack
- **Backend**: .NET 8, ASP.NET Core Identity, Entity Framework Core, MediatR, JWT Authentication.
- **Frontend**: React (Vite), TypeScript, Tailwind CSS, Axios, Context API.
- **Database**: PostgreSQL (Production/Dev/CI).
- **Infrastructure & CI**: GitHub Actions, Docker, Nginx (Reverse Proxy), Cloudflare.

---

## 2. Developer Experience (DevEx) & Setup

To streamline the local environment setup, this project includes PowerShell automation scripts.

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
    > **Rider Tip**: You can also right-click the API project and select **Manage \> User Secrets** to edit the JSON file directly.
3.  Apply migrations: `dotnet ef database update`.
4.  Run: `dotnet run`.

-----

## 3. Continuous Integration & Quality (SDET Focus)

This project features a robust CI pipeline via **GitHub Actions** that enforces quality on every push to the `dev` and `release` branches.

  - **Automated Testing**:
      - Executes **88 tests** (Unit and Integration).
      - Uses **PostgreSQL Service Containers** in GitHub Actions to ensure integration tests run against a real database environment.
  - **Linting & Build**:
      - Strict ESLint checks for the frontend.
      - Production build validation for both stacks.

-----

## 4. Security & Architecture

  - **Secret Management**: Zero sensitive data in source control.
  - **Clean Architecture**: Decoupled layers for Domain, Application, Infrastructure, and API.
  - **Identity & JWT**: Secure authentication flow with role-based access control (RBAC).
  - **Sequential Testing**: Integration tests run sequentially to prevent database race conditions.

-----

## 5. Project Roadmap

The detailed evolution of this project, including future tracks like Playwright E2E testing and Advanced Search, can be found [here](https://github.com/HCioffi42/henrique-cioffi-portfolio/blob/dev/docs/roadmap.md).
