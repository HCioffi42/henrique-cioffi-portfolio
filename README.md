# MyPersonalSite - Professional Blog & CMS

This repository contains a full-stack personal blog system built with a Clean Architecture approach using .NET for the backend and React for the frontend.

## 1. Tech Stack
- **Backend**: .NET 8, ASP.NET Core Identity, Entity Framework Core, MediatR, JWT Authentication.
- **Frontend**: React (Vite), TypeScript, Tailwind CSS, Axios, Context API.
- **Database**: PostgreSQL (Production/Dev), SQLite (Testing).

---

## 2. Getting Started

### 2.1 Backend Setup
1. Navigate to the `MeuSitePessoal.Api` directory.
2. Ensure the connection string in `appsettings.json` points to your PostgreSQL instance.
3. Apply migrations to initialize the Identity schema:
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
3. Create a environment file (see **Configuration** section below).
4. Start the development server:
   ```bash
   npm run dev
   ```

---

## 3. Configuration

The frontend requires environment variables to communicate with the API. Create a `.env.development` and a `.env.production` file in the root of the frontend project.

```env
# Base URL for the C# Backend API
VITE_API_URL=http://localhost:25683/api
```

**Note**: The `.env` files are ignored by Git. A `.env.example` file is provided as a template for new environments.

---

## 4. Testing

The solution includes a comprehensive test suite with both Unit and Integration tests.

### 4.1 Running Tests
To run the full suite via CLI:
```bash
dotnet test
```

### 4.2 Important Note on Integration Tests
Integration tests involve database operations and migrations. To prevent race conditions and conflicts in the shared test database (`meusitepessoal_testdb`), **parallel execution is disabled** at the assembly level.

The following configuration in the test project ensures sequential execution:
```csharp
[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

---

## 5. Security Features (Track 04)
- **Identity Integration**: Uses ASP.NET Core Identity for secure user and password management.
- **JWT Authentication**: Stateless authentication via JSON Web Tokens.
- **Route Guarding**: Frontend routes are protected via a `ProtectedRoute` component that validates the session state before rendering.
- **Axios Interceptors**: 
    - Automatically injects the Bearer token into requests.
    - Handles `401 Unauthorized` responses by clearing the local session and redirecting to the login page.
