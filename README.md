# TaskTracker API

[![CI](https://github.com/Pavithra729/TaskTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/Pavithra729/TaskTracker/actions/workflows/ci.yml)

A REST API for managing tasks, built with ASP.NET Core 10 and Entity Framework Core. It includes automated tests, a Docker image, and a CI pipeline that builds, tests and smoke-tests the running container on every push.

## Tech stack

- **.NET 10 / ASP.NET Core Web API** with controllers
- **Entity Framework Core 10** with **SQLite**, using code-first migrations
- **xUnit** tests running against an in-memory SQLite database
- **Docker** multi-stage build, running as a non-root user
- **GitHub Actions** for continuous integration

## Endpoints

| Method | Route             | Description          | Success          | Errors |
|--------|-------------------|----------------------|------------------|--------|
| GET    | `/api/tasks`      | List all tasks       | `200 OK`         |        |
| GET    | `/api/tasks/{id}` | Get one task         | `200 OK`         | `404`  |
| POST   | `/api/tasks`      | Create a task        | `201 Created`    | `400`  |
| PUT    | `/api/tasks/{id}` | Update title/status  | `204 No Content` | `404`  |
| DELETE | `/api/tasks/{id}` | Delete a task        | `204 No Content` | `404`  |

### Example

```http
POST /api/tasks
Content-Type: application/json

{ "title": "Learn Docker" }
```

```json
{
  "id": 1,
  "title": "Learn Docker",
  "isDone": false,
  "createdAt": "2026-11-08T17:20:00Z"
}
```

The server sets `id`, `isDone` and `createdAt`. Clients only supply `title` (and `isDone` when updating). A successful POST also returns a `Location` header pointing to the new task.

Ready-to-run requests for every endpoint are in [`src/TaskTracker.Api/TaskTracker.Api.http`](src/TaskTracker.Api/TaskTracker.Api.http) (use the REST Client extension in VS Code, or Visual Studio / Rider).

## Running locally

**Prerequisite:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/Pavithra729/TaskTracker.git
cd TaskTracker
dotnet run --project src/TaskTracker.Api
```

The API starts on the URL shown in the console (`http://localhost:5177` by default). The SQLite database (`tasks.db`) and its tables are created automatically on first run, because migrations are applied at startup.

## Running the tests

```bash
dotnet test
```

The tests call each controller action directly against a fresh in-memory SQLite database, covering both the success path and the 404 path for every endpoint. Each test gets its own empty database, so tests are isolated and can run in any order.

## Running with Docker

```bash
docker build -t tasktracker-api .
docker run -p 8080:8080 tasktracker-api
```

Then open `http://localhost:8080/api/tasks`.

Inside the container the database is stored in `/app/data`. To keep data after the container is removed, mount a volume:

```bash
docker run -p 8080:8080 -v tasktracker-data:/app/data tasktracker-api
```

## CI pipeline

Every push and pull request to `main` runs [`.github/workflows/ci.yml`](.github/workflows/ci.yml), which has two jobs:

1. **build-and-test**: restores, builds in Release mode and runs the full test suite.
2. **docker** (runs only if tests pass): builds the Docker image, starts the container, and smoke-tests the live API by creating a task and checking that it is returned. Container logs are printed even on failure, to help with debugging.

## Project structure

```
TaskTracker/
├── src/TaskTracker.Api/          # Web API project
│   ├── Controllers/              # TasksController
│   ├── Data/                     # AppDbContext
│   ├── Migrations/               # EF Core migrations
│   ├── Models/                   # TaskItem entity
│   └── Program.cs                # DI setup, middleware, startup migrations
├── tests/TaskTracker.Api.Tests/  # xUnit tests
├── Dockerfile
├── .github/workflows/ci.yml
└── TaskTracker.slnx
```

## Design notes

- **Server-controlled fields:** `Id`, `IsDone` (on create) and `CreatedAt` are set by the server, never taken from the request body.
- **Migrations on startup:** the app applies pending EF Core migrations when it starts, so a fresh clone or a new container works without running `dotnet ef` manually.
- **Real database in tests:** tests use in-memory SQLite rather than EF Core's InMemory provider, so they run real SQL and enforce constraints like `NOT NULL`.
- **Configuration by environment:** the connection string lives in `appsettings.json` and is overridden in the container with the `ConnectionStrings__Default` environment variable, with no code changes.