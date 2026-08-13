# WorxForge Backend

The backend API for the WorxForge platform — a community-driven repository where teams share, discover, and reuse AI prompts, agents, workflows, and coding standards.

Built with ASP.NET Core 10, Clean Architecture, EF Core, SignalR, and Google OAuth 2.0.

---

## Table of Contents

- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Configuration Reference](#configuration-reference)
- [Google OAuth Setup](#google-oauth-setup)
- [Database Setup](#database-setup)
- [Running the API](#running-the-api)
- [Debug Login (Development)](#debug-login-development)
- [Running Tests](#running-tests)
- [API Overview](#api-overview)
- [Contributing](#contributing)

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Database | SQL Server + EF Core 10 |
| Auth | Google OAuth 2.0 + JWT Bearer |
| Real-time | SignalR |
| Logging | Serilog |
| API Docs | Swagger UI (Swashbuckle) |

---

## Architecture

The solution follows Clean Architecture with four projects:

```
WorxForge.Domain         — entities, commands, queries, DTOs, errors
WorxForge.Core           — handlers, validators, service & repository interfaces
WorxForge.Infrastructure — EF Core DbContext, repositories, services, migrations
WorxForge.API            — ASP.NET Core minimal API, endpoint groups, middleware
```

Dependencies flow inward: `API → Core → Domain` and `Infrastructure → Core → Domain`. No layer references its outer caller.

---

## Prerequisites

- [.NET SDK 10.0.x](https://dotnet.microsoft.com/download) — `global.json` pins `10.0.302`, rolling forward to the latest patch
- SQL Server — any edition (Express, Developer, LocalDB, Azure SQL)
- Git
- PowerShell 5.1+ (Windows) **or** Bash (Linux / macOS)
- A Google Cloud project with OAuth 2.0 credentials _(required for real Google sign-in; optional if only using the development debug-login endpoint)_

---

## Quick Start

### 1. Clone the repository

```bash
git clone https://github.com/your-org/WorxForge-BE.git
cd WorxForge-BE
```

### 2. Run the setup script

The setup script handles everything: tool restore, NuGet restore, build, user-secret prompts, and an optional database migration.

**Windows (PowerShell)**

```powershell
.\setup.ps1
```

**Linux / macOS (Bash)**

```bash
chmod +x setup.sh
./setup.sh
```

The script will:

1. Verify the .NET SDK is installed
2. Restore local .NET tools (including `dotnet-ef`)
3. Restore and evaluate NuGet packages
4. Build the solution
5. Initialize user secrets and interactively prompt for the three required values
6. Offer to apply EF Core database migrations immediately

> **CI / non-interactive environments:** pass `--non-interactive` (Bash) or `-NonInteractive` (PowerShell). Missing required secrets are reported as warnings instead of prompts.

---

## Configuration Reference

WorxForge uses [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) so that no credentials are committed to source control.

The setup script handles the prompts. To set or update a value manually:

```powershell
dotnet user-secrets set "<Key>" "<Value>" --project src\WorxForge.API
```

### Required secrets

| Key | Description | Example |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | `Server=localhost;Database=WorxForge;Trusted_Connection=True;TrustServerCertificate=True` |
| `Cors:AllowedOrigins:0` | Frontend origin the browser is allowed to call from (no trailing slash). The API refuses to start if this is missing or empty. | `http://localhost:5173` |

### Optional secrets

| Key | Description | Default in `appsettings.Development.json` |
|---|---|---|
| `Authentication:Google:ClientId` | Google OAuth 2.0 Client ID. Required for real Google sign-in; the `debug-login` endpoint works without it. | _(none)_ |
| `JwtAccessToken:Issuer` | JWT issuer claim | `WorxForge.Dev` |
| `JwtAccessToken:Audience` | JWT audience claim | `WorxForge.Client.Dev` |
| `JwtAccessToken:SigningKey` | HMAC-SHA256 signing key — minimum 32 characters | `WorxForgeDevelopmentSigningKey123!` |
| `JwtAccessToken:ExpirationMinutes` | Token lifetime in minutes | `60` |
| `developmentGoogleSubject` | Google subject used by `debug-login` to resolve/create your local test user | `11111111-1111-1111-1111-111111111111` |
| `developmentEmail` | Email used by `debug-login` to resolve/create your local test user | `dev.local@example.com` |

> JWT and debug-login values are already set in `appsettings.Development.json` and work out of the box. Override them via user-secrets only if you need different values.

### Multiple CORS origins

```powershell
dotnet user-secrets set "Cors:AllowedOrigins:0" "http://localhost:5173" --project src\WorxForge.API
dotnet user-secrets set "Cors:AllowedOrigins:1" "http://localhost:3000" --project src\WorxForge.API
```

### Verify all secrets are set

```powershell
dotnet user-secrets list --project src\WorxForge.API
```

---

## Google OAuth Setup

> Skip this section if you only need the `debug-login` development endpoint.

1. Go to [Google Cloud Console](https://console.cloud.google.com/) and create or select a project.
2. Navigate to **APIs & Services → Library**, search for **Google Identity**, and enable it.
3. Go to **APIs & Services → Credentials → Create Credentials → OAuth 2.0 Client ID**.
4. Set the application type to **Web application**.
5. Under **Authorized JavaScript origins**, add your frontend origin (e.g. `http://localhost:5173`).
6. Click **Create** and copy the **Client ID**.
7. Save it as a user secret:

```powershell
dotnet user-secrets set "Authentication:Google:ClientId" "<your-client-id>" --project src\WorxForge.API
```

The backend validates Google ID tokens server-side using `Google.Apis.Auth`. No Google credentials are stored.

---

## Database Setup

EF Core Code-First migrations manage the schema. The setup script offers to apply them automatically. To run them manually:

**Windows**

```powershell
dotnet ef database update `
  --project src\WorxForge.Infrastructure `
  --startup-project src\WorxForge.API `
  --context WorxForge.Infrastructure.Configuration.WorxForgeDbContext
```

**Linux / macOS**

```bash
dotnet ef database update \
  --project src/WorxForge.Infrastructure \
  --startup-project src/WorxForge.API \
  --context WorxForge.Infrastructure.Configuration.WorxForgeDbContext
```

### Notes

- Use a dedicated database (e.g. `WorxForge`) — never use `master`.
- No database files are stored in this repository; the schema lives in your SQL Server instance.
- Run `dotnet ef database update` after every `git pull` to apply any new migrations.
- If you see `PendingModelChangesWarning` at startup, run `dotnet ef migrations add <Name>` before updating.
- Lookup tables (asset types, domains, use cases, tools) are seeded automatically on startup — no manual data import needed.

---

## Running the API

```powershell
cd src\WorxForge.API
dotnet run
```

The API starts on:

- `https://localhost:7235`
- `http://localhost:5222`

Swagger UI is available at `https://localhost:7235/swagger` in the `Development` environment.

---

## Debug Login (Development)

Real Google sign-in requires a valid Google ID token issued by a frontend. To get a JWT during backend-only development:

```
POST /api/authentication/debug-login
```

This endpoint:
- is **only registered in the `Development` environment** — it does not exist in production
- creates or resolves a local test user using `developmentGoogleSubject` and `developmentEmail`
- returns a JWT you can paste into Swagger UI's **Authorize** dialog

The default values in `appsettings.Development.json` work immediately after setup with no extra configuration.

---

## Running Tests

```powershell
dotnet test src\WorxForge.slnx
```

---

## API Overview

All routes are prefixed with `/api`.

| Group | Base path | Auth required |
|---|---|---|
| Authentication | `/api/authentication` | Partial (login is public) |
| Assets | `/api/assets` | Partial (reads are public) |
| Explore | `/api/explore` | No |
| Home | `/api/home` | Yes |
| Library | `/api/library` | Yes |
| Profile | `/api/profile` | Partial |
| Notifications | `/api/notifications` | Yes |
| Settings | `/api/settings` | Yes |
| Admin | `/api/admin` | Yes (Admin role) |

Full interactive documentation is available through Swagger UI at runtime.

---

## Contributing

1. Fork the repository.
2. Create a feature branch off `Dev`:
   ```bash
   git checkout Dev
   git checkout -b feature/your-feature-name
   ```
3. Ensure the solution builds and tests pass.
4. Open a pull request targeting the `Dev` branch.

Please use [Conventional Commits](https://www.conventionalcommits.org/) for commit messages (`feat:`, `fix:`, `chore:`, `refactor:`, etc.).
