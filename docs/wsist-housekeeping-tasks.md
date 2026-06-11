# WSIST — Housekeeping, CI & Account Deletion

Branch: `feat/housekeeping`

Four commits. Run `dotnet build` before each commit. No migrations needed.

---

## Commit 1 — Fix the production error page

**What:** `Program.cs` registers `app.UseExceptionHandler("/Error")` but `Error.razor` doesn't exist. An unhandled exception in production currently re-executes to a 404. Fix it.

### Create `WSIST/WSIST.Web/Components/Pages/Error.razor`

```razor
@page "/Error"
@layout EmptyLayout
@using WSIST.Web.Components.Layout
<PageTitle>Error – WSIST</PageTitle>

<div class="error-page">
    <div class="error-content">
        <span class="error-icon">⚠</span>
        <h1 class="error-title">Something went wrong</h1>
        <p class="error-sub">An unexpected error occurred. If this keeps happening, try logging out and back in.</p>
        <a href="/" class="button-primary button-accent" data-enhance-nav="false">← Back to home</a>
    </div>
</div>
```

### Add to `app.css`

```css
/* ── Error Page ── */

.error-page {
    min-height: 100dvh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--bg);
}

.error-content {
    text-align: center;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 16px;
    padding: 2rem;
    max-width: 400px;
}

.error-icon {
    font-size: 40px;
    color: var(--accent);
}

.error-title {
    font-size: 1.4rem;
    font-weight: 700;
    letter-spacing: -0.02em;
}

.error-sub {
    font-size: 14px;
    color: var(--text-secondary);
    line-height: 1.6;
}
```

**Commit:**
```
fix: add missing Error.razor to resolve production 404 on unhandled exceptions
```

---

## Commit 2 — GitHub Actions CI

**What:** Nothing currently runs automatically on push or PR. Add a workflow that builds the solution and runs all 12 unit tests.

### Create `.github/workflows/ci.yml`

```yaml
name: CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore WSIST/WSIST.sln

      - name: Build
        run: dotnet build WSIST/WSIST.sln --no-restore --configuration Release

      - name: Test
        run: dotnet test WSIST/WSIST.UnitTests/WSIST.UnitTests.csproj --configuration Release --logger "console;verbosity=normal"
```

**Commit:**
```
ci: add GitHub Actions workflow — build and test on push and PR
```

---

## Commit 3 — docker-compose, task file cleanup, README

### Step 1 — Track docker-compose

The `docker-compose.yml` is already on disk but untracked. If the file exists, add it as-is:

```bash
git add WSIST/docker-compose.yml
```

If for any reason the file is missing, create `WSIST/docker-compose.yml`:

```yaml
services:
  mysql:
    image: mysql:8.0
    container_name: wsist-mysql
    restart: unless-stopped
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD:-password}
      MYSQL_DATABASE: ${MYSQL_DATABASE:-wsistdb}
    ports:
      - "3306:3306"
    volumes:
      - wsist-mysql-data:/var/lib/mysql
    healthcheck:
      test: ["CMD-SHELL", "mysqladmin ping -h localhost -uroot -p$$MYSQL_ROOT_PASSWORD"]
      interval: 10s
      timeout: 5s
      retries: 10

volumes:
  wsist-mysql-data:
```

### Step 2 — Archive completed task files

Create a `docs/` directory. Move any `.md` task files in the repo root (wsist-tasks.md, wsist-tasks-2.md, wsist-tasks-3.md, wsist-tasks-4.md, wsist-phase3-tasks.md, wsist-redesign-tasks.md, or similar) into `docs/`. If none exist at root, skip this step.

### Step 3 — Rewrite `README.md`

Replace the full contents of `README.md` with:

```markdown
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
```

**Commit:**
```
chore: track docker-compose, archive task files, rewrite README
```

---

## Commit 4 — Self-service account deletion

**What:** The privacy policy promises data deletion on request but there's no self-service option. Add a deletion flow to the Settings page — two-step confirmation, full cascade delete of tests and custom subjects, then sign-out.

### Step 1 — Add `DeleteUser` to `TestManagement.cs`

```csharp
public void DeleteUser(int userId)
{
    var user = context.Users.Find(userId);
    if (user is null) return;
    context.Users.Remove(user);
    context.SaveChanges();
}
```

EF Core will cascade-delete all tests and custom subjects for the user automatically (already configured in `WsistContext.OnModelCreating`). System subjects (IsSystem = true, UserId = null) are unaffected.

### Step 2 — Add deletion state and logic to `Settings.razor.cs`

Add two fields:

```csharp
private bool showDeleteConfirm = false;
private bool isDeleting = false;
```

Add two methods:

```csharp
private void ShowDeleteConfirm() => showDeleteConfirm = true;
private void CancelDelete() => showDeleteConfirm = false;

private void ConfirmDeleteAccount()
{
    isDeleting = true;
    management.DeleteUser(CurrentUserId);
    navigation.NavigateTo("/logout", forceLoad: true);
}
```

### Step 3 — Add the danger zone section to `Settings.razor`

Add this block at the very bottom of the settings content div, after the subjects block:

```razor
<div class="settings-block">
    <p class="settings-block-label">Account</p>
    <div class="settings-card">
        @if (!showDeleteConfirm)
        {
            <div class="settings-row settings-row-last">
                <div>
                    <span class="settings-row-key">Delete account</span>
                    <p style="font-size: 13px; color: var(--text-muted); margin-top: 3px;">
                        Permanently removes your account and all test data.
                    </p>
                </div>
                <button class="button-primary button-danger" @onclick="ShowDeleteConfirm">
                    Delete account
                </button>
            </div>
        }
        else
        {
            <div class="delete-confirm-zone">
                <p class="delete-confirm-title">Are you sure?</p>
                <p class="delete-confirm-sub">
                    This will permanently delete your account, all @allTests.Count test@(allTests.Count == 1 ? "" : "s"), and all custom subjects. This cannot be undone.
                </p>
                <div class="delete-confirm-actions">
                    <button class="button-primary" @onclick="CancelDelete" disabled="@isDeleting">
                        Cancel
                    </button>
                    <button class="button-primary button-danger" @onclick="ConfirmDeleteAccount" disabled="@isDeleting">
                        @(isDeleting ? "Deleting…" : "Yes, delete everything")
                    </button>
                </div>
            </div>
        }
    </div>
</div>
```

### Step 4 — Add CSS to `app.css`

```css
.delete-confirm-zone {
    padding: 20px 18px;
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.delete-confirm-title {
    font-size: 15px;
    font-weight: 600;
    color: #f87171;
}

.delete-confirm-sub {
    font-size: 14px;
    color: var(--text-secondary);
    line-height: 1.5;
}

.delete-confirm-actions {
    display: flex;
    gap: 8px;
    justify-content: flex-end;
}
```

### Verify

- Navigate to Settings — "Account" section appears with a "Delete account" button
- Click it — confirmation panel appears with the test count and a Cancel option
- Click Cancel — returns to the normal state
- Click "Yes, delete everything" — button changes to "Deleting…", user is deleted, redirected to `/login-page`
- Attempting to log in again with the same Google account creates a fresh empty account (expected)

**Commit:**
```
feat: add self-service account deletion to settings, closes privacy policy gap
```

---

## Done

Push `feat/housekeeping` and open a PR. After merging, purge the Cloudflare cache.
