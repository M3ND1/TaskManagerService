# TaskManagerService

TaskManagerService is a .NET 8 Web API that lets teams create, assign, and track tasks with JWT-secured endpoints, refresh tokens, and PostgreSQL persistence. The codebase spotlights clean layering, FluentValidation, AutoMapper, MediatR (CQRS), and EF Core repositories with unit and integration coverage.

## Highlights

- **Layered architecture** – Controllers delegate to MediatR handlers in Application, which use repository interfaces and `TaskManagerDbContext` implementations.
- **Security-first auth** – JWT bearer tokens, refresh token rotation with chaining, Argon2 password hashing via `IPasswordService`, and policy-based authorization (`Admin`, `User`, `TaskEditor`).
- **Validation + mapping** – FluentValidation guards every DTO with centralized rules, while AutoMapper keeps controllers slim.
- **CQRS via MediatR** – Commands and Queries separated per feature module under `Features/`.
- **Testing** – Dedicated unit and integration projects under `test/` keep regressions in check.

## Architecture at a glance

```
core/
  TaskManager.Api           // Controllers, exception handlers, DI setup
  TaskManager.Application   // MediatR handlers, DTOs, services, validators, mapping profiles
  TaskManager.Core          // Entities, enums, repository interfaces, custom exceptions
  TaskManager.Infrastructure// EF Core DbContext, repository implementations, auth helpers
test/
  IntegrationTests/         // Database migration & repository integration tests
  UnitTests/                // Application handler & core unit tests
```

PostgreSQL is provided via the included `docker-compose.yml` (exposed on port **5433**).

## Tech stack

- .NET 8, C# 12
- ASP.NET Core Minimal Hosting model
- EF Core 9 + Npgsql (PostgreSQL 15)
- MediatR (CQRS)
- FluentValidation, AutoMapper
- JWT Bearer auth + custom refresh token rotation
- Argon2 password hashing (Konscious.Security.Cryptography)
- Docker (PostgreSQL, SonarQube)

## Getting started

1. **Prerequisites** – .NET 8 SDK, Docker (optional but recommended).
2. **Start PostgreSQL**
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

Or focus on a layer, e.g. `test/UnitTests/Application.UnitTests` for handler coverage or `test/IntegrationTests/Infrastructure.IntegrationTests` for repository interactions.

## API endpoints

### Auth & Users

| Method | Route                | Description                            | Auth |
| ------ | -------------------- | -------------------------------------- | ---- |
| POST   | `/api/user/register` | Create user with hashed password       | No   |
| POST   | `/api/user/login`    | Issue JWT + refresh token pair         | No   |
| POST   | `/api/user/refresh`  | Rotate tokens (refresh token chaining) | No   |
| GET    | `/api/user`          | Get authenticated user profile         | Yes  |
| PUT    | `/api/user`          | Update authenticated user              | Yes  |
| DELETE | `/api/user`          | Delete authenticated user              | Yes  |

### Tasks

| Method | Route                         | Description                            | Auth |
| ------ | ----------------------------- | -------------------------------------- | ---- |
| POST   | `/api/task`                   | Create task tied to authenticated user | Yes  |
| GET    | `/api/task/{id}`              | Get task by ID                         | Yes  |
| PUT    | `/api/task/{id}`              | Update task (timestamps tracked)       | Yes  |
| DELETE | `/api/task/{id}`              | Delete task                            | Yes  |
| GET    | `/api/task/{id}/tags`         | Get tags assigned to a task            | Yes  |
| POST   | `/api/task/{id}/tags/{tagId}` | Assign tag to task                     | Yes  |
| DELETE | `/api/task/{id}/tags/{tagId}` | Remove tag from task                   | Yes  |

### Tags

| Method | Route           | Description              | Auth |
| ------ | --------------- | ------------------------ | ---- |
| POST   | `/api/tag`      | Create tag               | Yes  |
| GET    | `/api/tag`      | Get all tags (paginated) | Yes  |
| GET    | `/api/tag/{id}` | Get tag by ID            | Yes  |
| PUT    | `/api/tag/{id}` | Update tag               | Yes  |
| DELETE | `/api/tag/{id}` | Delete tag               | Yes  |

## Reviewer cheat sheet

- MediatR handlers under `Features/` show the business rules (timestamps, refresh-token rotation, exception flow).
- `ServiceCollectionExtensions.AddJwtAuthentication` centralizes auth configuration.
- Validators like `CreateManagedTaskDtoValidator` illustrate the FluentValidation style guide.
- `.github/copilot-instructions.md` documents the repo's strict coding expectations.

## Code quality

**SonarQube** runs locally for static analysis, code smells, and security hotspots.

```bash
docker compose -f docker-compose.sonarqube.yml up -d
./scripts/sonar-scan.sh
# View at http://localhost:9000 (admin/admin)
```

**SonarLint** integration in VS Code provides real-time feedback bound to the local SonarQube instance.
