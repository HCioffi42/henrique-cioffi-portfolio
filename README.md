# MyPersonalSite - Professional Blog & CMS

This repository contains a full-stack personal blog system built with a Clean Architecture approach using .NET for the backend and React for the frontend.

## 1. Tech Stack
- **Backend**: .NET 8, ASP.NET Core Identity, Entity Framework Core, MediatR, JWT Authentication.
- **Frontend**: React (Vite), TypeScript, Tailwind CSS, Axios, Context API.
- **Database**: PostgreSQL (Production/Dev/CI).
- **Infrastructure & CI**: GitHub Actions, Docker (Postgres Service Containers).

---

## 2. Getting Started

### 2.1 Backend Setup
1. Navigate to the `MeuSitePessoal.Api` directory.
2. **Initialize Local Secrets**: The connection string is no longer stored in `appsettings.json` for security reasons. Configure it in your local Secret Manager:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=MeuSiteDb;Username=postgres;Password=YOUR_PASSWORD;Include Error Detail=true"
   ```
   > **Rider Tip**: You can also right-click the API project and select **Manage > User Secrets** to edit the JSON file directly.
3. Apply migrations to initialize the database:
   ```bash
   dotnet ef database update
   ```
4. Run the project:
   ```bash
   dotnet run
   ```

### 2.2 Frontend Setup
1. Navigate to the `meupessoal-web` directory.
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm run dev
   ```

---

## 3. Continuous Integration (Track 11.1)

This project features a robust CI pipeline via **GitHub Actions** that triggers on every push or PR to the `dev` branch.

- **Backend Validation**: 
    - Executes **88 automated tests** (Unit and Integration).
    - Spins up a temporary **PostgreSQL Docker container** to run integration tests against a real database instance, ensuring environment parity.
- **Frontend Validation**: 
    - Runs strict **ESLint** checks to enforce code quality and React best practices.
    - Validates the production build to prevent deployment failures.

---

## 4. Configuration & Environment

### 4.1 Backend Secrets
Sensitive data such as JWT Keys and Database Passwords are managed via:
- **Development**: .NET Secret Manager (`secrets.json`).
- **CI**: GitHub Actions Secrets & Environment Variables.

### 4.2 Frontend Variables
Create a `.env.development` file in the frontend root:
```env
VITE_API_URL=http://localhost:25683/api
```

---

## 5. Testing Architecture

### 5.1 Integration Tests
To prevent race conditions and conflicts in the shared test database (`meusitepessoal_testdb`), **parallel execution is disabled** at the assembly level.

The following configuration in the test project ensures sequential execution:
```csharp
[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

### 5.2 Running Tests Locally
```bash
dotnet test
```

---

## 6. Security Features
- **Secret Manager**: Zero sensitive data stored in source control.
- **Identity Integration**: Secure user and password management via ASP.NET Core Identity.
- **JWT Authentication**: Stateless authentication with automatic session expiration.
- **Route Guarding**: Protected administrative routes on the frontend.
- **Axios Interceptors**: Global handling of authentication tokens and `401 Unauthorized` responses.
