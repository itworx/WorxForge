# WorxForge

**The ITWorx AI Assets Hub**

Discover · Contribute · Share · Reuse

WorxForge is an open-source AI assets hub for sharing and discovering reusable
prompts, agents, skills, workflows, and coding standards. This monorepo contains
an ASP.NET Core API and a React web app. The source includes generic lookup data;
it does not include external login accounts, mailboxes, credentials, or a populated
organization knowledgebase.

## The story behind WorxForge

WorxForge began as a four-week ITWorx summer internship project in Summer 2026.
Four undergraduate computer science students built and demonstrated the initial
application with AI coding tools and senior mentorship.

The whitepaper **Junior Developers Can Build with AI Coding Agents** describes
the learning approach, mentor-reported outcomes, and evidence limits of this
single-cohort case study. The current repository includes later release preparation
changes; see [feature status](docs/feature-status.md) for current capabilities.

[Read the whitepaper](docs/whitepaper/whitepaper.md) ·
[Download the PDF](docs/whitepaper/WorxForge-Case-Study.pdf)

## What you can do

- Explore, search, and filter assets; view asset details and Markdown previews.
- Publish, edit, and delete guest-owned assets.
- Bookmark assets in My Library, add comments and reactions, and view your profile.
- Receive in-app notifications through SQL Server persistence and SignalR delivery.

See [feature status](docs/feature-status.md) for available and unfinished features.

## Guest access and current limitations

Login is skipped by default in every environment. Open the app and navigate
Home, Explore, My Library, My Assets, Publish Asset, Profile, and Notifications
without signing in. The API supplies a synthetic **Community Guest** identity
with no email address or credentials.

**All visitors share the guest profile, assets, bookmarks, reactions, and
notifications.** Guest-owned assets can be edited or deleted by another visitor.
Use a fresh database containing public/demo data. Per-user accounts and private
workspaces require completing authentication and account flows. Administrator
authorization remains restricted; guest mode does not grant administrator access.

**TODO: Login is temporarily skipped until an authentication provider is configured
and account flows are completed. Email notifications are skipped until an SMTP
account is configured.**

Email delivery is registered as a successful no-op: no SMTP network connection,
sender address, or password is required. Email-change/verification endpoints return
`status: "skipped"` without changing or falsely verifying an email address.
Enabling email requires implementing and registering an SMTP sender as well as
configuring an account; SMTP credentials alone do not enable delivery.

## Stack and source layout

| Component | Location | Stack |
|---|---|---|
| API | `src/WorxForge.API` | ASP.NET Core 10, minimal APIs, SignalR |
| Application | `src/WorxForge.Core` | Handlers, validators, contracts |
| Domain | `src/WorxForge.Domain` | Entities, DTOs, commands and queries |
| Infrastructure | `src/WorxForge.Infrastructure` | EF Core 10, SQL Server, repositories |
| Web | `src/WorxForge.UI` | React 19, TypeScript, Vite 8, Tailwind CSS, TanStack Query |
| Tests | `src/Tests` | xUnit, Moq, FluentAssertions |

See [architecture](docs/architecture.md) for the data flow.

## Prerequisites

- Git, and a terminal opened at the repository root after cloning.
- .NET SDK 10.0.302, or a later patch in the same SDK feature band allowed by
  [global.json](global.json). An older .NET 10 SDK does not satisfy this pin.
- Node.js 24 and npm; [.nvmrc](.nvmrc) specifies v24.18.1.
- A reachable SQL Server instance and a fresh development database. Windows users
  can use [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb).
  On macOS/Linux, use a reachable SQL Server instance rather than LocalDB.
- PowerShell on Windows, or Bash on macOS/Linux.

## Quick start

Clone the repository and enter its root:

```sh
git clone https://github.com/itworx/WorxForge.git
cd WorxForge
```

Run `./setup.ps1` on Windows or `bash setup.sh` on macOS/Linux. Both scripts restore
and build the backend, prompt for local configuration, offer database migrations,
and install the frontend. `-BackendOnly` / `--backend-only` and `-FrontendOnly` /
`--frontend-only` are supported. No Google or SMTP account is needed for guest mode.

Use the startup commands below after setup. The scripts do not start the API;
starting the frontend alone is insufficient. If you skipped migrations during
setup, apply them before starting the API.

### Backend: manual setup and startup

Run these commands from the repository root in PowerShell or Bash. Replace the
connection-string placeholder with your own fresh development database connection
string. The database login must have permission to apply schema migrations.

```sh
dotnet restore src/WorxForge.slnx
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your fresh database connection string>" --project src/WorxForge.API
dotnet ef database update --project src/WorxForge.Infrastructure --startup-project src/WorxForge.API --context WorxForge.Infrastructure.Configuration.WorxForgeDbContext -- --environment Development
dotnet dev-certs https --trust
dotnet run --project src/WorxForge.API --launch-profile https
```

The API uses the `knowledge-hub-local-development` User Secrets namespace.
The migration command explicitly selects Development so local User Secrets and
development CORS configuration are loaded. The tool manifest includes EF Core 10
migration tooling and NSwag. Generic lookups and the guest row are seeded when the
API starts after migrations; the source does not provide sample knowledge assets.

The HTTPS launch profile matches the frontend's default proxy target:
`https://localhost:7235`. It also listens on `http://localhost:5222`. Swagger is
available in Development at `https://localhost:7235/swagger`.
For certificate issues, follow Microsoft's
[development certificate guidance](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0#trust-the-aspnet-core-https-development-certificate).
For local HTTP instead, run with `--launch-profile http` and set
`VITE_API_PROXY_TARGET=http://localhost:5222` in the frontend's `.env.local`.

### Frontend: startup in a second terminal

From the repository root:

```sh
cd src/WorxForge.UI
```

Copy the template once, preserving any existing local settings:

```powershell
# Windows / PowerShell
Copy-Item .env.example .env.local
```

```sh
# macOS / Linux / Bash
cp .env.example .env.local
```

Then install dependencies and start Vite:

```sh
npm ci
npm run dev
```

Open `http://localhost:5173`. Leave `VITE_API_BASE_URL` empty for local development;
Vite forwards `/api` and `/hubs` to `VITE_API_PROXY_TARGET`. Keep the frontend on
port 5173, or update `Cors:AllowedOrigins:0` to match the actual frontend origin.
Google client configuration is unused while guest access is enabled.

## Configuration and hosting

Keep real connection strings and credentials in local User Secrets, environment
variables, or ignored local files. Never commit `.env.local`, database content, or
private documents. Environment variables use `__` for nested keys.
Values beginning with `VITE_` are embedded in the client build and must not contain
secrets.

| Setting | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | API SQL Server connection string |
| `Cors__AllowedOrigins__0` | Allowed frontend origin; required outside Development |
| `GuestAccess__Enabled` | Defaults to `true`; all visitors use the shared guest identity |
| `VITE_API_PROXY_TARGET` | Local Vite API/SignalR proxy target; defaults to HTTPS port 7235 |
| `VITE_API_BASE_URL` | Public API origin for separate-origin deployments; set before building |

Development allows `http://localhost:5173`. A deployed API requires an explicit
connection string and CORS origin; local User Secrets are for Development.
Build the frontend with `npm run build` and serve `src/WorxForge.UI/dist` with SPA
route fallback. Vite's development proxy is not included in that build. For a
shared origin, configure the host to forward `/api` and `/hubs`, including WebSocket
traffic, to the API. For separate origins, set `VITE_API_BASE_URL` and API CORS to
match. Configure HTTPS at the host; setup does not deploy or publish the app.

### Optional authentication

The frontend follows `GET /api/authentication/access-mode`. Guest mode ignores
incoming bearer tokens, clears a previous browser login token, does not expose
Google/debug login endpoints, and needs no JWT signing key.

To use the retained Google/JWT flow, explicitly set `GuestAccess:Enabled=false`
and configure `Authentication:Google:ClientId`, frontend `VITE_GOOGLE_CLIENT_ID`,
and `JwtAccessToken:Issuer`, `Audience`, `SigningKey`, and `ExpirationMinutes`.
Use your own signing key of at least 32 bytes. Debug login is available only in
Development with guest mode disabled and separately configured
`developmentGoogleSubject` / `developmentEmail`.

The retained login flow does not complete the unfinished password, account,
email, or admin features. Authentication-provider work and SMTP integration are
separate tasks; see [feature status](docs/feature-status.md).

## Validation

From the repository root:

```sh
dotnet test src/WorxForge.slnx --configuration Release
dotnet list src/WorxForge.slnx package --vulnerable --include-transitive
cd src/WorxForge.UI
npm ci
npm audit --audit-level=high
npm run lint
npm test -- --maxWorkers=1
npm run build
```

With the API and Vite running against a development database, optionally run
`npm run test:publish:smoke` from `src/WorxForge.UI`. It verifies upload, creation,
read, update, and deletion of a temporary asset. It defaults to
`http://127.0.0.1:5173`; set `WORXFORGE_WEB_URL` to test a different web origin.

Backend and frontend CI run for changes to their source and can also be manually
dispatched. Some integration/system tests remain skipped placeholders; passing
unit tests does not establish that every unfinished feature works. The release
workflow is manually dispatched from `main` and does not deploy the app.

## Contributing

Fork the repository, create a branch for your change, and open a pull request
against `main`. Describe the behavior changed and the checks you ran. Update
documentation when changing setup, configuration, or feature availability.
Use generic demo content; contribute only code and assets you have permission to
share publicly, and keep credentials, private documents, and database exports out
of commits. Retain the ITWORX copyright and license notices.

## Before public publication

The current source tree is sanitized, but the existing private clone's Git history
still contains removed private files and configuration. Deleting or ignoring a
file does not remove it from previous commits or branches. Use a clean source
snapshot with new history, or sanitize and audit every ref that will become public
before changing the existing repository's visibility.
See [publication notes](docs/publication.md) for the publication procedure.

## License and attribution

Copyright (c) 2026 ITWORX. Licensed under the [MIT License](LICENSE).
Reuse, modification, distribution, and commercial use are permitted provided the
copyright and permission notices are retained in copies or substantial portions.
Retain the license file when forking or redistributing the project. The license
requires notice retention; it does not require a website backlink or endorsement.
