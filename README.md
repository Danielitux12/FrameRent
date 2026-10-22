# FrameRent

Equipment Rental Management System built with **.NET 10 (LTS)** following **Clean Architecture** principles.

## Solution Architecture

```
FrameRent/
├── src/
│   ├── FrameRent.Domain/          # Core domain models, Value Objects (DateRange, RentalCost), Enums, Repository contracts
│   ├── FrameRent.Application/     # DTOs, Use cases/services (Auth, Equipment, Rental lifecycle, Reports), Exceptions
│   ├── FrameRent.Infrastructure/  # EF Core DbContext, Configurations, Identity, JWT tokens, Repositories, Migrations
│   └── FrameRent.Api/             # REST API Controllers, Global exception middleware, Swagger OpenAPI, Startup seeding
└── FrameRent.sln
```

## Features

- **Domain Model & State Machine**:
  - Encapsulated lifecycle for rentals: `Pending` -> `Approved` -> `Delivered` -> `Returned` (with late fee calculation) / `Rejected` / `Cancelled`.
  - Owned types for `DateRange` and `RentalCost` (`Total` and `LateFee`).
  - Active commitments and concurrency protection on inventory units.
  - Per-client active rental quotas.
- **Security & Authorization**:
  - ASP.NET Core Identity with custom `ApplicationUser`.
  - JWT Bearer authentication with claims and role-based policies (`Client`, `Staff`).
- **REST API**:
  - Interactive Swagger / OpenAPI documentation with JWT Bearer support.
  - Global `ExceptionHandlingMiddleware` transforming exceptions into RFC 7807 `ProblemDetails`.

## Seed Credentials

When starting the application with a connected database, default roles and an administrative staff account are seeded automatically:

| Field | Value |
|---|---|
| **Staff Email** | `staff@framerent.com` |
| **Staff Password** | `Staff#2024!` |
| **Staff Full Name** | `FrameRent Staff` |
| **Role** | `Staff` |

> [!NOTE]
> Client users can register themselves via `POST /api/auth/register`, which always assigns the `Client` role.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [SQL Server](https://www.microsoft.com/sql-server/) (local instance or Docker container)

### Build & Run

1. Restore dependencies and build the solution:
   ```bash
   dotnet restore
   dotnet build
   ```

2. Configure connection strings in `src/FrameRent.Api/appsettings.json` if necessary.

3. Update the database schema via EF Core Migrations:
   ```bash
   dotnet ef database update --project src/FrameRent.Infrastructure --startup-project src/FrameRent.Api
   ```

4. Run the API project:
   ```bash
   dotnet run --project src/FrameRent.Api
   ```

5. Access Swagger UI at `https://localhost:<port>/swagger` (or `http://localhost:<port>/swagger`).
