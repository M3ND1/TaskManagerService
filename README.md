# TaskManagerService

TaskManagerService is a .NET 8 Web API that lets teams create, assign, and track tasks with JWT-secured endpoints, refresh tokens, and PostgreSQL persistence. The codebase spotlights clean layering, FluentValidation, AutoMapper, and EF Core repositories with unit and integration coverage.

## Highlights

- **Layered architecture** – Controllers delegate to Application services, which use repository interfaces and `TaskManagerDbContext` implementations.
- **Security-first auth** – JWT bearer tokens, refresh token rotation, password hashing via `IPasswordService`, and policy-based authorization (`Admin`, `User`, `TaskEditor`).
- **Validation + mapping** – FluentValidation guards every DTO, while AutoMapper keeps controllers slim.
- **Testing** – Dedicated unit and integration projects under `test/` keep regressions in check.

## Architecture at a glance

```
core/
  TaskManager.Api           // Controllers, exception handlers, DI setup
  TaskManager.Application   // DTOs, services, validators, mapping profiles
  TaskManager.Core          // Entities, enums, repository interfaces
  TaskManager.Infrastructure// EF Core DbContext, repository implementations, auth helpers
```

PostgreSQL is provided via the included `docker-compose.yml`.

## Tech stack

- .NET 8, C# 12
- ASP.NET Core Minimal Hosting model
- EF Core + Npgsql
- FluentValidation, AutoMapper
- JWT Bearer auth + custom refresh tokens
- Docker (PostgreSQL)

## Getting started

1. **Prerequisites** – .NET 8 SDK, Docker (optional but recommended).
2. **Start PostgreSQL (optional)**
   ```bash
   docker compose up -d postgres
   ```
3. **Restore & build**
   ```bash
   dotnet build
   ```
4. **Apply migrations**
   ```bash
   dotnet ef database update --project core/TaskManager.Infrastructure --startup-project core/TaskManager.Api
   ```
5. **Run the API**
   ```bash
   dotnet run --project core/TaskManager.Api
   ```
   Swagger UI is available at `https://localhost:5001/swagger` in Development.

## Tests

Run everything:

```bash
dotnet test
```

Or focus on a layer, e.g. `test/UnitTests/Application.UnitTests` for service coverage or `test/IntegrationTests/Database.IntegrationTests` for EF interactions.

## API quick peek

- `POST /api/auth/register` – create users with hashed passwords.
- `POST /api/auth/login` – issue JWT + refresh token pair.
- `POST /api/auth/refresh` – rotate tokens via `UserService.RefreshTokenForUserAsync`.
- `POST /api/task` – create tasks tied to the authenticated user (see `ManagedTaskService`).
- `PUT/DELETE /api/task/{id}` – update or remove tasks with optimistic concurrency handling.

## Reviewer cheat sheet

- `ManagedTaskService` & `UserService` show the business rules (timestamps, refresh-token rotation, exception flow).
- `ServiceCollectionExtensions.AddJwtAuthentication` centralizes auth configuration.
- Validators like `CreateManagedTaskDtoValidator` illustrate the FluentValidation style guide.
- `.github/copilot-instructions.md` documents the repo’s strict coding expectations.

## Next steps

- Extend task queries (filters, pagination) via repository interfaces.
- Expand integration tests to cover refresh-token revocation edge cases.
