# WorxForge Backend

Backend API for the WorxForge platform, built with ASP.NET Core and organized for Clean Architecture.

## What this repository contains

- ASP.NET Core API
- Core application abstractions
- Domain entities, commands, queries, and DTOs
- Infrastructure layer with EF Core and supporting services
- Unit, integration, and system test projects
- Repository-level build and setup configuration

## Repository structure

```
/
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── README.md
├── setup.ps1
├── setup.sh
├── docs/
│   ├── architecture/
│   ├── requirements/
│   └── archive/
└── src/
    ├── WorxForge.API/
    ├── WorxForge.Core/
    ├── WorxForge.Domain/
    ├── WorxForge.Infrastructure/
    └── Tests/
```

## Prerequisites

- .NET SDK 10.0.302 or compatible `10.0.x`
- Git
- SQL Server instance for local development
- PowerShell 5.1+ on Windows or Bash on Linux/macOS

## Quick start

### 1) Clone the repository

```bash
git clone <repo-url>
cd WorxForge.Backend
```

### 2) Run the setup script

#### Windows

```powershell
.\setup.ps1
```

#### Linux / macOS

```bash
chmod +x setup.sh
./setup.sh
```

The setup scripts:

- verify the .NET SDK is installed
- restore local .NET tools
- restore centrally managed NuGet packages
- build the solution
- warn about any missing required user-secrets (see below)

## Local development configuration

This repository uses ASP.NET Core User Secrets for local-sensitive settings.

### API project secrets

Use these commands from the repository root:

```powershell
dotnet user-secrets init --project .\src\WorxForge.API\WorxForge.API.csproj
dotnet user-secrets set --project .\src\WorxForge.API\WorxForge.API.csproj "ConnectionStrings:DefaultConnection" "Data Source=localhost;Initial Catalog=WorxForge;Integrated Security=True;Pooling=False;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Name=vscode-mssql;Application Intent=ReadWrite;Command Timeout=30"
dotnet user-secrets set --project .\src\WorxForge.API\WorxForge.API.csproj "JwtAccessToken:Issuer" "WorxForge.Dev"
dotnet user-secrets set --project .\src\WorxForge.API\WorxForge.API.csproj "JwtAccessToken:Audience" "WorxForge.Client.Dev"
dotnet user-secrets set --project .\src\WorxForge.API\WorxForge.API.csproj "JwtAccessToken:SigningKey" "<your-dev-signing-key>"
dotnet user-secrets set --project .\src\WorxForge.API\WorxForge.API.csproj "developmentGoogleSubject" "dev-debug-google-subject-yourname"
dotnet user-secrets set --project .\src\WorxForge.API\WorxForge.API.csproj "developmentEmail" "your.name@itworx.com"
dotnet user-secrets set --project .\src\WorxForge.API\WorxForge.API.csproj "Cors:AllowedOrigins:0" "http://localhost:5173"
```

These two development keys are used by the `/api/authentication/debug-login` and `/api/authentication/debug-login-helper` endpoints to create or resolve your local test user.

`Cors:AllowedOrigins` lists the frontend origin(s) allowed to call the API from the browser (no trailing slash). The API fails to start if this is missing or empty — set it to wherever your frontend dev server runs (e.g. `http://localhost:5173` for the default Vite React app).

### Verify secrets

```powershell
dotnet user-secrets list --project .\src\WorxForge.API\WorxForge.API.csproj
```

## Database setup

The database is created through EF Core migrations against your local SQL Server instance.

### Apply the initial migration

```powershell
dotnet ef database update --project .\src\WorxForge.Infrastructure\WorxForge.Infrastructure.csproj --startup-project .\src\WorxForge.API\WorxForge.API.csproj --context WorxForge.Infrastructure.Configuration.WorxForgeDbContext
```

### Notes

- Use a dedicated app database such as `WorxForge`
- Do not use `master`
- No database file is stored in the repository
- The schema is created in the SQL Server instance you point the connection string at
- Re-run `dotnet ef database update` after every pull — new migrations aren't applied automatically on startup. If you see `PendingModelChangesWarning`, the model has drifted from the last migration; run `dotnet ef migrations add <Name>` first.

## NSwag client generation

NSwag is restored as a local .NET tool when you run the setup script.

After setup, you can invoke it from the repository root with:

```powershell
dotnet tool restore
dotnet tool run nswag
```

## Build

```powershell
dotnet build .\src\WorxForge.slnx
```

## Documentation

- [`docs/requirements/`](docs/requirements/) — SRS, BRD, and database design
- [`docs/architecture/`](docs/architecture/) — architecture notes and decisions
- [`docs/archive/`](docs/archive/) — historical and final handoff documents

## Current setup status

- Repository foundation is in place
- Central package management is enabled
- User Secrets are configured on the API project
- EF Core migration baseline exists
- Seed data exists for documented lookup tables
