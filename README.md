# WorxForge

WorxForge is a community-driven platform where teams share, discover, and reuse AI knowledge assets — prompts, agents, workflows, and coding standards.

This is the **WorxForge monorepo**: it contains both the backend API and the web frontend, sharing a single history and issue tracker.

| Part | Location | Stack |
|---|---|---|
| **Backend** | [`src/`](src/) | ASP.NET Core 10, Clean Architecture, EF Core, SignalR, Google OAuth 2.0 |
| **Frontend** | [`src/WorxForge.UI/`](src/WorxForge.UI/) | React 19 + TypeScript, Vite 8, Tailwind CSS v4, TanStack Query |

> This repository was formed by merging the previously separate `WorxForge-BE` and `WorxForge-FE` repositories, **preserving the full commit history of both**. See [`docs/repository-consolidation.md`](docs/repository-consolidation.md).

---

## Table of Contents

- [Repository Structure](#repository-structure)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Backend Development](#backend-development)
- [Frontend Development](#frontend-development)
- [Connecting Frontend and Backend](#connecting-frontend-and-backend)
- [Testing](#testing)
- [CI/CD](#cicd)
- [Contributing](#contributing)
- [Contributors](#contributors)

---

## Repository Structure

```
WorxForge/
├── src/                          # Backend (.NET solution: WorxForge.slnx) + frontend
│   ├── WorxForge.API/            # ASP.NET Core minimal API, endpoints, middleware, SignalR hubs
│   ├── WorxForge.Core/           # Handlers, validators, service & repository interfaces
│   ├── WorxForge.Domain/         # Entities, commands, queries, DTOs, errors
│   ├── WorxForge.Infrastructure/ # EF Core DbContext, repositories, services, migrations
│   ├── WorxForge.UI/             # Frontend (Vite + React + TypeScript)
│   │   ├── src/                  # Feature-based application source
│   │   ├── public/
│   │   └── package.json
│   └── Tests/                    # Unit / Integration / System tests
├── .config/dotnet-tools.json     # Local .NET tools (dotnet-ef, ...)
├── Directory.Build.props         # Shared MSBuild properties
├── Directory.Packages.props      # Central NuGet package management
├── global.json                   # Pinned .NET SDK
├── .nvmrc                        # Pinned Node.js version
├── .editorconfig                 # Shared editor config (backend + frontend scoped)
├── .gitignore
├── setup.ps1 / setup.sh          # Unified setup (backend + frontend)
└── README.md
```

---

## Architecture

### Backend

Clean Architecture with four projects and inward-flowing dependencies:

```
WorxForge.Domain         — entities, commands, queries, DTOs, errors
WorxForge.Core           — handlers, validators, service & repository interfaces
WorxForge.Infrastructure — EF Core DbContext, repositories, services, migrations
WorxForge.API            — ASP.NET Core minimal API, endpoint groups, middleware
```

Dependencies flow inward: `API → Core → Domain` and `Infrastructure → Core → Domain`. No layer references its outer caller.

### Frontend

The app in `src/WorxForge.UI/` uses a feature-based folder structure:

```
src/WorxForge.UI/src/
  features/           # One folder per domain (assets, auth, explore, home, ...)
    <feature>/
      components/     # UI components scoped to this feature
      hooks/          # React Query hooks
      models/         # TypeScript types
      pages/          # Route-level components
      services/       # HTTP service functions
  layouts/            # AuthLayout, MainLayout (authenticated shell + sidebar)
  routes/             # appRouter.tsx — all route definitions
  contexts/           # AuthContext, NotificationContext, SessionContext
  api/                # NSwag-generated HTTP client + custom fetch wrapper
  components/         # Shared UI components (Pagination, SearchBar, AppSidebar)
```

The dev server proxies `/api/*` requests to the backend via Vite's built-in proxy (`VITE_API_PROXY_TARGET`). SignalR connects to `{VITE_API_BASE_URL}/hubs/notifications` using the access token.

---

## Prerequisites

| Requirement | Version / Notes |
|---|---|
| .NET SDK | 10.0.x — `global.json` pins `10.0.302` (rolls forward to latest patch) |
| SQL Server | any edition (Express, Developer, LocalDB, Azure SQL) — backend only |
| Node.js | v24 — see `.nvmrc` |
| npm | bundled with Node.js |
| Git | any recent version |
| PowerShell 5.1+ **or** Bash | for the setup scripts |
| Google Cloud project | OAuth 2.0 Client ID (shared by backend and frontend) |

---

## Quick Start

Clone and run the unified setup script from the repository root.

**Windows (PowerShell)**

```powershell
.\setup.ps1                # sets up backend AND frontend
.\setup.ps1 -BackendOnly   # backend only
.\setup.ps1 -FrontendOnly  # frontend only
```

**Linux / macOS (Bash)**

```bash
chmod +x setup.sh
./setup.sh                 # sets up backend AND frontend
./setup.sh --backend-only
./setup.sh --frontend-only
```

The script runs backend setup (tool + NuGet restore, build, user-secret prompts, optional EF migration) and frontend setup (env config, `npm ci`, build, optional lint).

> **CI / non-interactive:** pass `-NonInteractive` (PowerShell) or `--non-interactive` (Bash). Missing backend secrets are reported as warnings; the frontend `.env.local` is copied from `.env.example` as-is.

---

## Backend Development

### Configuration (User Secrets)

The backend uses [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) so no credentials are committed. The setup script prompts for these; to set one manually:

```powershell
dotnet user-secrets set "<Key>" "<Value>" --project src/WorxForge.API
```

**Required secrets**

| Key | Description | Example |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | `Server=localhost;Database=WorxForge;Trusted_Connection=True;TrustServerCertificate=True` |
| `Cors:AllowedOrigins:0` | Frontend origin allowed by CORS (no trailing slash). The API refuses to start if missing. | `http://localhost:5173` |

**Optional secrets** (defaults in `appsettings.Development.json` work out of the box)

| Key | Description |
|---|---|
| `Authentication:Google:ClientId` | Google OAuth 2.0 Client ID (real Google sign-in; `debug-login` works without it) |
| `JwtAccessToken:Issuer` / `Audience` / `SigningKey` / `ExpirationMinutes` | JWT settings |
| `developmentGoogleSubject` / `developmentEmail` | Local test user used by `debug-login` |

### Database

EF Core Code-First migrations manage the schema (the setup script offers to apply them). Manually:

```powershell
dotnet ef database update `
  --project src/WorxForge.Infrastructure `
  --startup-project src/WorxForge.API `
  --context WorxForge.Infrastructure.Configuration.WorxForgeDbContext
```

### Running the API

```powershell
cd src/WorxForge.API
dotnet run
```

- API: `https://localhost:7235` / `http://localhost:5222`
- Swagger UI (Development): `https://localhost:7235/swagger`

### Debug Login (Development only)

`POST /api/authentication/debug-login` returns a JWT for backend-only development without a real Google token. Registered only in the `Development` environment.

---

## Frontend Development

Environment variables live in `src/WorxForge.UI/.env.local` (git-ignored). Copy `src/WorxForge.UI/.env.example` to start.

| Variable | Description | Required |
|---|---|---|
| `VITE_API_BASE_URL` | Backend API base URL used by the browser at runtime. Leave empty in local dev to use the Vite proxy. | No (empty = proxy) |
| `VITE_API_PROXY_TARGET` | Vite dev proxy target — `/api` requests are forwarded here; also the SignalR hub base. | Yes (local dev) |
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth 2.0 Client ID from Google Cloud Console. | Yes |

```bash
cd src/WorxForge.UI
npm run dev        # http://localhost:5173
npm run build      # production build -> src/WorxForge.UI/dist/
```

---

## Connecting Frontend and Backend

For local development, point the frontend proxy at the running backend:

```env
# src/WorxForge.UI/.env.local
VITE_API_PROXY_TARGET=https://localhost:7235
```

Vite forwards all `/api/*` requests from the frontend dev server to that URL, so no CORS configuration is needed locally. Run the backend (see above), then the frontend.

The frontend's typed API client is generated from the backend's OpenAPI document via NSwag (`src/WorxForge.UI/generate-api.ps1`).

---

## Testing

**Backend**

```powershell
dotnet test src/WorxForge.slnx
```

**Frontend**

```bash
cd src/WorxForge.UI
npm test              # vitest run
npm run test:watch    # watch mode
npm run lint          # oxlint
```

---

## CI/CD

No CI/CD workflows are configured yet. When added, backend and frontend pipelines should be **path-filtered** so each runs only when its part of the tree changes (e.g. `src/WorxForge.API/**`, `src/WorxForge.Core/**`, etc. for backend; `src/WorxForge.UI/**` for frontend).

---

## Contributing

1. Create a feature branch off the default development branch.
2. Ensure the affected side builds and its tests pass.
3. Use [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `chore:`, `refactor:`, ...).
4. Open a pull request.

---

## Contributors

1. Aliaa Eissa — https://www.linkedin.com/in/alyaaeissa2005/
2. Aya Sherif — https://www.linkedin.com/in/aya-sherif-56721b326/
3. Amr Atif — https://www.linkedin.com/in/amr-atif-68b121363/
4. Mahmoud Tarek — https://www.linkedin.com/in/mahmoud-tarek-5549a2375/
