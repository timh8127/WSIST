# WSIST — What Should I Study Today

WSIST (*What Should I Study Today*) is a Blazor Server-based study planner designed to help students organize tests, track understanding, and prioritize what to study based on urgency and preparedness.

This project was built as part of a software engineering apprenticeship (Informatiker EFZ, Applikationsentwicklung) and focuses on clean architecture, separation of concerns, and maintainable state management.

---

## Overview

WSIST helps students:

- Track upcoming tests
- Monitor their level of understanding per test
- Estimate workload and urgency
- Organize study priorities
- Persist data per user in a MySQL database

The system is designed with a strong separation between:

- UI layer (Blazor Server Web App)
- Business logic layer (WSIST.Engine)
- Persistence layer (EF Core + MySQL)

---

## Features

### Current

- Google OAuth authentication (login/logout)
- Per-user data isolation
- Create tests with:
  - Title
  - Subject
  - Due date
  - Volume (workload estimate)
  - Personal understanding level
  - Grade (only after due date)
- Edit existing tests
- Delete tests
- Persistent storage using MySQL via Entity Framework Core
- Clean state management via dependency injection
- Modal-based editing and creation UI
- Middleware-based auth guard on protected routes
- Dedicated login page with empty layout
- Fully testable business logic

### Planned

- Priority calculation algorithm
- "What should I study today" recommendation engine
- Study history tracking
- Analytics dashboard
- Cloud deployment

---

## Architecture
WSIST
│
├── WSIST.Web        → Blazor Server UI
├── WSIST.Engine     → Business logic + EF Core
└── WSIST.UnitTests  → NUnit test project

### Responsibilities

#### WSIST.Web

Handles:

- UI rendering (Razor Components)
- User interaction and modal state
- Google OAuth flow
- Auth guard middleware
- Calling Engine services

Contains no business logic.

#### WSIST.Engine

Core system logic:

- TestManagement service (CRUD)
- GetOrCreateUser (first-time user provisioning)
- EF Core DbContext (Pomelo/MySQL)
- Domain models (Test, User)

Fully independent of UI.

#### WSIST.UnitTests

Contains unit tests for:

- Grade verification logic
- (Full overhaul planned — current tests need rewriting for DB-backed services)

Uses NUnit.

---

## Technology Stack

### Frontend

- Blazor Server
- Razor Components
- InteractiveServer render mode (prerender disabled)
- Bootstrap 5 (CDN)

### Backend

- C# .NET 10
- ASP.NET Core minimal API endpoints
- Dependency Injection
- Middleware pipeline

### Authentication

- Google OAuth 2.0
- ASP.NET Core Cookie Authentication
- Credentials stored via .NET User Secrets (never committed)

### Persistence

- MySQL
- Entity Framework Core 9
- Pomelo.EntityFrameworkCore.MySql

### Testing

- NUnit

---

## Design Principles

Key architectural goals:

- Separation of concerns
- Testable business logic
- UI independent from data layer
- Clean state management
- Auth enforced at middleware level, not just component level

Avoids:

- Hidden state
- Tight UI-logic coupling
- Committing secrets to version control

---

## Getting Started

### Requirements

- .NET 10 SDK
- MySQL server running locally
- Google OAuth credentials (Client ID + Secret)

### Setup

**1. Create the database**

Make sure MySQL is running with a database called `wsistdb` accessible at `localhost` with user `root`.

**2. Configure secrets**
```bash
cd WSIST/WSIST.Web
dotnet user-secrets set "Google:ClientId" "your-client-id"
dotnet user-secrets set "Google:ClientSecret" "your-client-secret"
```

**3. Apply migrations**
```bash
dotnet ef database update --project WSIST.Engine --startup-project WSIST.Web
```

**4. Run the project**
```bash
dotnet run --project WSIST/WSIST.Web
```

Then open:
http://localhost:7165

---

## Auth Flow

1. Unauthenticated user hits `/` → middleware redirects to `/login-page`
2. User clicks "Continue with Google" → Google OAuth flow
3. On success → redirected to `/` → user provisioned in DB if first time
4. Logout hits `/logout` endpoint → cookie cleared → redirected to `/login-page`

---

## Testing

Run tests:
```bash
dotnet test
```

> Note: Unit tests are currently marked for overhaul. The two DB-dependent tests (`TestIfNewTestGetsMade`, `CheckIfTestWasDeleted`) need to be rewritten to work with an in-memory or mocked DB context. The grade verification tests (`CheckIfGradeIsNotNullIfInThePast`, `CheckIfGradeIsNullIfInTheFuture`) are still valid.

---

## Project Structure
WSIST/
│
├── WSIST.Web/
│   ├── Components/
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor
│   │   │   └── EmptyLayout.razor
│   │   ├── Pages/
│   │   │   ├── Home.razor
│   │   │   ├── Home.razor.cs
│   │   │   └── Login.razor
│   │   ├── App.razor
│   │   └── Routes.razor
│   ├── wwwroot/
│   │   └── app.css
│   └── Program.cs
│
├── WSIST.Engine/
│   ├── Test.cs
│   ├── User.cs
│   ├── TestManagement.cs
│   ├── TestAssistants.cs
│   └── WsistContext.cs
│
├── WSIST.UnitTests/
│   └── UnitTests.cs
│
└── README.md

---

## Security Notes

- Google credentials are stored in .NET User Secrets locally and must never be committed
- `appsettings.Development.json` is gitignored
- Auth is enforced via ASP.NET Core middleware before any Blazor component renders
- Each user only sees their own tests (filtered by `UserId` in all DB queries)

---

## Author

Tim Hug

---

## Status

Actively developed