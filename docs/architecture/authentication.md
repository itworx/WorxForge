# Authentication — Implementation Reference

## Overview

Google-only OAuth 2.0 authentication for the WorxForge backend MVP.

- Users sign in via Google. No email/password flow exists.
- A short-lived JWT is issued on every successful login.
- Every authenticated request is validated against a persisted `AUTH_SESSION` row.
- Logout revokes the session server-side, making the JWT immediately invalid even before it expires.

---

## Flow

```
Client                   API                          DB
  |                        |                           |
  |--- POST /login -------->|                           |
  |    { idToken: "..." }   |                           |
  |                        |-- ValidateAsync(idToken) ->|
  |                        |<- GooglePayload -----------|
  |                        |-- GetByGoogleId() -------->|
  |                        |<- User (or null) ----------|
  |                        |   (fallback: GetByEmail)   |
  |                        |-- INSERT AUTH_SESSION ----->|
  |<-- 200 { accessToken } |                           |
  |                        |                           |
  |--- GET /me ------------>|                           |
  |    Authorization: Bearer <jwt>                      |
  |                        |-- GetById(sessionId) ----->|
  |                        |<- Session -----------------|
  |                        |   (validate not revoked)   |
  |<-- 200 { profile }     |                           |
  |                        |                           |
  |--- POST /logout ------->|                           |
  |    Authorization: Bearer <jwt>                      |
  |                        |-- UPDATE revoked_at ------>|
  |<-- 204                 |                           |
```

---

## Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/authentication/login` | None | Google ID token → JWT |
| POST | `/api/authentication/logout` | Required | Revokes active session |
| GET | `/api/authentication/me` | Required | Returns current user profile |
| POST | `/api/authentication/debug-login` | None (Dev only) | Issues JWT for test user |
| POST | `/api/authentication/debug-login-helper` | None (Dev only) | Same + Swagger-ready fields |

---

## JWT Claims

| Claim | Value | Note |
|-------|-------|------|
| `sub` | User GUID | Mapped via `ClaimConstants.UserId = "sub"` |
| `sid` | Session GUID | Used for per-request session lookup |
| `email` | User email | |
| `jti` | Random GUID | Token uniqueness |
| `name` | Display name | Optional, omitted if empty |
| `role` | `User` / `Admin` | |
| `exp` | UTC timestamp | Controlled by `JwtAccessToken:ExpirationMinutes` |
| `iss` | Configurable | `JwtAccessToken:Issuer` |
| `aud` | Configurable | `JwtAccessToken:Audience` |

**Important:** `MapInboundClaims = false` is set on the JWT bearer options. Without this, ASP.NET Core remaps `sub` → `ClaimTypes.NameIdentifier` and `sid` → a long URI claim, breaking session lookup.

---

## Session Validation (per-request)

Every request to a protected endpoint triggers `OnTokenValidated`:

1. Extract `sub` (userId) and `sid` (sessionId) from JWT claims.
2. Query `AUTH_SESSION` by `sessionId`.
3. Reject if: session not found / userId mismatch / `RevokedAt != null` / `ExpiresAt <= now`.

This means a revoked JWT is rejected **immediately**, even if it hasn't expired.

---

## Account Resolution Logic (Google Login)

Order of operations in `AuthenticateWithGoogleAsync`:

1. Look up user by `GoogleId` (Google subject string) → found → issue token.
2. Look up user by email → not found → provision new user from Google payload → issue token.
3. Look up user by email → found, `GoogleId` is null → link Google account → issue token.
4. Look up user by email → found, `GoogleId` is different → reject with error.

---

## Configuration (user-secrets)

```json
{
  "Authentication:Google:ClientId": "<google-oauth-client-id>",
  "JwtAccessToken:Issuer": "WorxForge.Dev",
  "JwtAccessToken:Audience": "WorxForge.Client.Dev",
  "JwtAccessToken:SigningKey": "<min-32-char-key>",
  "JwtAccessToken:ExpirationMinutes": 60,
  "ConnectionStrings:DefaultConnection": "<sql-server-connection-string>",
  "developmentGoogleSubject": "dev-debug-google-subject-yourname",
  "developmentEmail": "your.name@itworx.com"
}
```

Set via:
```powershell
dotnet user-secrets set "<key>" "<value>" --project src/WorxForge.API
```

Example for team members:
```powershell
dotnet user-secrets init --project src/WorxForge.API/WorxForge.API.csproj
dotnet user-secrets set --project src/WorxForge.API/WorxForge.API.csproj "developmentGoogleSubject" "dev-debug-google-subject-alyaa"
dotnet user-secrets set --project src/WorxForge.API/WorxForge.API.csproj "developmentEmail" "alyaa.eissa@itworx.com"
dotnet user-secrets list --project src/WorxForge.API/WorxForge.API.csproj
```

**JWT signing key must be at least 32 characters** (256 bits) for HS256.

---

## Files Changed / Created

### New Files

| File | Purpose |
|------|---------|
| `src/WorxForge.API/Configuration/BearerSecurityOperationFilter.cs` | Adds `Bearer` security requirement to authorized Swagger operations |
| `src/WorxForge.Infrastructure/Services/AuthenticationService.cs` | Core auth business logic (Google + JWT + session) |
| `src/WorxForge.Infrastructure/Repositories/AuthSessionRepository.cs` | AUTH_SESSION CRUD |
| `src/WorxForge.Infrastructure/Repositories/UserRepository.cs` | User data access including `GetByGoogleIdAsync` |
| `src/WorxForge.Infrastructure/Options/JwtAccessTokenOptions.cs` | Typed options for JWT config |

### Modified Files

| File | Change |
|------|--------|
| `src/WorxForge.API/Program.cs` | Added `AddSwaggerGen` with Bearer definition + security requirement; registered auth, authorization, core, infrastructure |
| `src/WorxForge.API/Configuration/AuthenticationConfigurationExtensions.cs` | JWT bearer setup with `MapInboundClaims = false` and per-request session validation in `OnTokenValidated` |
| `src/WorxForge.API/EndpointGroups/AuthenticationEndpointGroup.cs` | Added `/login`, `/logout`, `/me`, `/debug-login`, `/debug-login-helper` endpoints |
| `src/WorxForge.API/Extensions/ApplicationBuilderExtensions.cs` | Added Swagger, SwaggerUI, OpenApi middleware in Development |
| `Directory.Packages.props` | Added `Google.Apis.Auth`, `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.OpenApi`, `Microsoft.OpenApi` |
| `src/WorxForge.Infrastructure/Migrations/` | Added `20260804224229_ChangeUserGoogleIdToString` migration |

---

## NuGet Packages Added

| Package | Version | Project | Purpose |
|---------|---------|---------|---------|
| `Google.Apis.Auth` | 1.75.0 | Infrastructure | Google ID token validation |
| `Swashbuckle.AspNetCore` | 10.2.3 | API | Swagger UI + OpenAPI generation |
| `Microsoft.AspNetCore.OpenApi` | 10.0.10 | API | ASP.NET Core OpenAPI integration |
| `Microsoft.OpenApi` | 2.7.5 | API | OpenAPI document model (v2, required by Swashbuckle v10) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.10 | API | JWT bearer middleware |

---

## Known Decisions

- **Google-only**: email/password, registration, password reset are `NotImplemented` by design for MVP.
- **`Microsoft.OpenApi` pinned to 2.7.5**: Swashbuckle v10 requires v2.x; v2.7.5 is the minimum floor that satisfies all transitive dependencies.
- **`OpenApiSecuritySchemeReference` requires `document` parameter**: Swashbuckle v10 / Microsoft.OpenApi v2 — passing `null` causes the security requirement to serialize as `{}`, breaking Swagger UI auth injection. Always pass the `OpenApiDocument` instance.
- **`MapInboundClaims = false`**: Required to prevent ASP.NET Core from remapping `sub` and `sid` JWT claims to their long `ClaimTypes.*` URI equivalents.
- **Debug endpoints are Development-only**: Checked via `IsDevelopment()` at route registration time, never compiled out.
- **Session revocation is immediate**: `OnTokenValidated` queries the DB on every request; a revoked JWT is rejected even before expiry.

---

## Review Closure Summary (2026-08-05)

The authentication MVP review comments were fully addressed.

### 1) Profile Ownership Boundary

`GetMyProfile` query flow was moved from Authentication slice to Profile slice:

- `src/WorxForge.Domain/Queries/Profile/GetMyProfileQuery.cs`
- `src/WorxForge.Core/Handlers/Profile/GetMyProfileQueryHandler.cs`
- `src/WorxForge.Core/Validators/Profile/GetMyProfileQueryValidator.cs`

Authentication endpoint wiring was updated to use the Profile query namespace.

### 2) Development Secrets (No Hardcoded Debug Identity)

Hardcoded debug values were removed from `AuthenticationService` and replaced with configuration/user-secrets keys:

- `developmentGoogleSubject`
- `developmentEmail`

If missing, the service now fails fast with a clear configuration error.

### 3) Username Provisioning Improvement

When auto-provisioning users from Google login (and debug-login creation), username is now derived from email local-part (before `@`), normalized, and collision-safe:

- base extraction from email local-part
- sanitized to `[a-z0-9._-]`
- lowercased and trimmed
- collision handling uses random small numeric suffixes (non-sequential)

This avoids exposing predictable user counts (`name2`, `name3`, ...).

### 4) Startup Hardening

JWT startup validation now explicitly checks:

- required issuer/audience/signing key
- `ExpirationMinutes > 0`
- signing key byte length >= 32 (256 bits)

### 5) Auth-Focused Tests

Auth-focused tests were updated from placeholders to meaningful coverage for handlers/validators, and authentication-focused test filters pass.

Note: full solution test runs still fail because of unrelated placeholder tests in other modules (Assets/System/Integration/Domain scaffolding).

---

## Scope Clarification

This auth delivery is complete for the agreed MVP scope:

- Google sign-in/login
- JWT issuance/validation
- session persistence + revocation checks
- logout
- `/me` retrieval through Profile slice ownership

Out of scope for this MVP (intentionally `NotImplemented`):

- email/password login
- registration via password
- forgot/reset password
- email verification flow

---

## Branch / Commit Trail (Auth Finalization)

- `06300e0` — read debug login identity from user secrets
- `4f8d13d` — move `GetMyProfile` query flow to Profile slice
- `2368013` — validate JWT expiration/signing key length at startup
- `f7227d4` — harden username generation and auth test coverage

---

## Next Session Handoff

Authentication backend is ready for MVP usage.

Next planned workstream: **Publish Asset feature**.
