# WSIST — What Should I Study Today

WSIST is a Blazor Server study planner that helps students decide what to focus on each day. Enter your upcoming tests, rate your understanding and the workload, and WSIST ranks them by priority — so you always know what matters most right now.

Live at **[wsist.forch.me](https://wsist.forch.me)**

---

## What it does

- Track upcoming tests with subject, due date, volume, understanding level, and eventual grade
- Priority engine scores each test across four dimensions (urgency, volume, understanding, past grade — 40 pts max)
- "What should I study today?" page recommends tests based on available hours
- Grade overview shows subject averages at a glance
- Custom subjects per user, in addition to system defaults
- Settings page for profile editing and subject management
- Full landing page with live interactive demo of the scoring algorithm

---

## Stack

| Layer | Implementation |
|---|---|
| UI | Blazor Server, .NET 10, Interactive Server render mode |
| Business logic | `WSIST.Engine` — PriorityCalculator, TestManagement, domain models |
| Persistence | EF Core 9 + Pomelo MySQL, code-first migrations, auto-applied on startup |
| Auth | Google OAuth 2.0 → ASP.NET Core cookie session |
| Deploy | Railway (Nixpacks, dotnet-sdk_10) behind Cloudflare CDN |
| Local dev | Docker Compose (MySQL 8.0) + .NET User Secrets |

---

## Architecture

```
WSIST/
├── WSIST.Web/        → Blazor Server UI, auth, API endpoints
├── WSIST.Engine/     → Business logic, EF Core DbContext, domain models
└── WSIST.UnitTests/  → NUnit tests (EF InMemory)
```

The engine has zero web dependencies. The web project consumes it via DI. Tests run entirely in-memory — no database required.

---

## Local setup

**Prerequisites:** .NET 10 SDK, Docker

**1. Start the database**
```bash
cd WSIST
docker compose up -d
```

**2. Configure secrets**
```bash
cd WSIST/WSIST.Web
dotnet user-secrets set "Google:ClientId" "your-client-id"
dotnet user-secrets set "Google:ClientSecret" "your-client-secret"
```

**3. Run**
```bash
dotnet watch --project WSIST/WSIST.Web --launch-profile https
```

Migrations apply automatically on startup. No `dotnet ef database update` needed.

---

## API

`GET /api/grades` — returns the authenticated user's average grade per subject.

```json
{
  "userId": 3,
  "subjects": [
    { "subjectId": 0, "subjectName": "Math", "averageGrade": 4.8, "gradedTestCount": 3 }
  ]
}
```

Requires an active session cookie. Returns 401 if unauthenticated.

---

## Priority scoring

Each test is scored out of 40 points:

| Factor | Max | Logic |
|---|---|---|
| Urgency | 10 | Days until due date |
| Volume | 12 | Self-rated workload |
| Understanding | 12 | Inverse of self-rated understanding |
| Grade pull | 6 | Subject average below passing threshold |

---

## Testing

```bash
dotnet test WSIST/WSIST.UnitTests
```

12 tests, all green. Engine-level coverage: CRUD, subject management, grade logic, priority scoring.

---

## Auth flow

1. Unauthenticated request to `/`, `/study`, or `/settings` → redirect to `/login-page`
2. "Continue with Google" → Google OAuth
3. Callback → user provisioned in DB if first visit → redirect to `/`
4. `/logout` → cookie cleared → redirect to `/login-page`

---

## Legal

Privacy policy and Terms of Use are available at [wsist.forch.me/privacy](https://wsist.forch.me/privacy) and [wsist.forch.me/terms](https://wsist.forch.me/terms).

---

## Author

Tim Hug — Informatiker EFZ, Applikationsentwicklung apprenticeship project
