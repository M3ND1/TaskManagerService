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

---

## Release 1.0 Roadmap

Current state: **~85% production-ready**. Core CRUD, auth, validation, and layering are solid. The phases below close the remaining gaps and harden the API for a confident 1.0 release.

### Phase 1 — Test Coverage (Critical)

The biggest gap before 1.0. Currently only Tag handlers and repository integration tests exist.

- [ ] **1.1** Unit tests for User command handlers (`CreateUserCommandHandler`, `LoginUserCommandHandler`, `UpdateUserCommandHandler`, `DeleteUserCommandHandler`)
- [ ] **1.2** Unit tests for User query handler (`GetUserQueryHandler`)
- [ ] **1.3** Unit tests for Task command handlers (`CreateTaskCommandHandler`, `UpdateTaskCommandHandler`, `DeleteTaskCommandHandler`)
- [ ] **1.4** Unit tests for Task query handlers (`GetTaskQueryHandler`, `GetTaskTagsQueryHandler`)
- [ ] **1.5** Unit tests for `RefreshTokenCommandHandler` (token rotation, revocation, expired token edge cases)
- [ ] **1.6** Unit tests for FluentValidation validators (all 8 validators — boundary values, error messages)
- [ ] **1.7** Unit tests for `PasswordService` (hash/verify round-trip, wrong password rejection)
- [ ] **1.8** Integration tests for `RefreshTokenRepository` (save, revoke, chain, expiry queries)
- [ ] **1.9** Fill `Infrastructure.UnitTests` placeholder — repository edge cases with in-memory provider

### Phase 2 — Task Filtering & Pagination

Tasks currently lack list/search capabilities. Tags already support `GetPagedAsync` — Tasks need parity.

- [ ] **2.1** Add `GetAllTasksAsync` and `GetPagedTasksAsync` to `IManagedTaskRepository` and implementation
- [ ] **2.2** Create `GetAllTasksQuery` MediatR handler with filtering: status (`IsCompleted`), priority, assignee, due date range
- [ ] **2.3** Add `GET /api/task` endpoint to `TaskController` with query parameters (`page`, `pageSize`, `isCompleted`, `priority`, `dueDateFrom`, `dueDateTo`, `assignedToId`)
- [ ] **2.4** Return `PagedResult<ManagedTaskResponseDto>` matching the existing `PagedResult<T>` pattern
- [ ] **2.5** Unit & integration tests for task filtering/pagination

### Phase 3 — Concurrency Control

The README mentions optimistic concurrency but it's not implemented. Critical for multi-user task updates.

- [ ] **3.1** Add `RowVersion` (xmin/timestamp) property to `ManagedTask` entity
- [ ] **3.2** Configure EF Core concurrency token in `TaskManagerDbContext`
- [ ] **3.3** Add `RowVersion` to `UpdateManagedTaskDto` and `ManagedTaskResponseDto`
- [ ] **3.4** Handle `DbUpdateConcurrencyException` in `UpdateTaskCommandHandler` — throw `ConflictException`
- [ ] **3.5** Add `ConflictException` + `ConflictExceptionHandler` returning 409
- [ ] **3.6** Migration for RowVersion column
- [ ] **3.7** Unit tests for concurrency conflict scenarios

### Phase 4 — API Hardening & Security

Production-grade security measures beyond auth.

- [ ] **4.1** Add rate limiting middleware on auth endpoints (`/register`, `/login`, `/refresh`) using `Microsoft.AspNetCore.RateLimiting` (built-in .NET 8)
- [ ] **4.2** Configure CORS policy in `Program.cs` — allow configurable origins from `appsettings.json`
- [ ] **4.3** Add request/response logging middleware with sensitive field redaction (passwords, tokens)
- [ ] **4.4** Add `X-Request-Id` correlation header propagation for traceability
- [ ] **4.5** Health check endpoint (`/health`) with DB connectivity check using `AspNetCore.HealthChecks.NpgSql`
- [ ] **4.6** SonarQube scan — resolve all Critical and High severity findings

### Phase 5 — API Polish & Developer Experience

Quality-of-life improvements for API consumers and maintainers.

- [ ] **5.1** API versioning via URL path (`/api/v1/`) using `Asp.Versioning.Http`
- [ ] **5.2** Structured logging with Serilog — console + file sinks, request context enrichment
- [ ] **5.3** Global `CancellationToken` timeout policy (configurable request timeout)
- [ ] **5.4** Enhance Swagger with XML documentation comments on all endpoints
- [ ] **5.5** Add `GET /api/user/{id}/tasks` — get all tasks assigned to a specific user (paginated)
- [ ] **5.6** Soft delete support — `IsDeleted` + `DeletedAt` on `ManagedTask` and `Tag` entities, global query filter in DbContext
- [ ] **5.7** Audit trail — `CreatedBy`/`UpdatedBy` fields on `ManagedTask` updates via a MediatR pipeline behavior

### Phase 6 — CI/CD & Release Packaging

Ship it.

- [ ] **6.1** GitHub Actions CI pipeline: restore → build → test → SonarQube scan
- [ ] **6.2** Dockerfile for the API (multi-stage build, non-root user, health check)
- [ ] **6.3** Update `docker-compose.yml` to include the API service alongside PostgreSQL
- [ ] **6.4** Environment-specific configuration: Production `appsettings.Production.json` with hardened defaults
- [ ] **6.5** Database migration strategy for deployment (auto-migrate on startup vs. separate migration step)
- [ ] **6.6** Tag release `v1.0.0`, generate changelog from commit history

---

### Phase priority & dependencies

```
Phase 1 (Tests)          ████████████████████  CRITICAL — unblocks confidence for all other phases
Phase 2 (Filtering)      ████████████████      HIGH — core feature gap
Phase 3 (Concurrency)    ████████████████      HIGH — data integrity
Phase 4 (Security)       ████████████████      HIGH — production readiness
Phase 5 (Polish)         ████████████          MEDIUM — DX and maintainability
Phase 6 (CI/CD)          ████████████          MEDIUM — deployment readiness
```

Phases 1–4 are **blocking for 1.0**. Phases 5–6 are **strongly recommended** but could ship as 1.1 if time-constrained.

### Estimated scope

| Phase           | Items        | Impact                                           |
| --------------- | ------------ | ------------------------------------------------ |
| 1 — Tests       | 9 tasks      | Fills all handler/validator/repository test gaps |
| 2 — Filtering   | 5 tasks      | Adds task list, search, and pagination           |
| 3 — Concurrency | 7 tasks      | Prevents lost updates on concurrent edits        |
| 4 — Security    | 6 tasks      | Rate limiting, CORS, health checks, logging      |
| 5 — Polish      | 7 tasks      | Versioning, soft deletes, audit trail, Serilog   |
| 6 — CI/CD       | 6 tasks      | Pipeline, Docker, release packaging              |
| **Total**       | **40 tasks** | **Complete 1.0 release**                         |
