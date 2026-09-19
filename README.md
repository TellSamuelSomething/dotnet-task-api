# dotnet-task-api

A task management app with an ASP.NET Core 10 REST API and a React frontend, built to demonstrate real-world backend patterns: authentication, layered architecture, tests, CI and background work.

Users register, sign in and manage their own tasks. Every task, category and real-time event is scoped to the signed-in user.

## Tech stack

**Backend**
- ASP.NET Core 10 Web API
- Entity Framework Core with SQLite and migrations
- JWT access tokens with rotating refresh tokens
- SignalR for real-time events
- Hangfire for background jobs
- OpenTelemetry traces and metrics
- xUnit unit and integration tests, GitHub Actions CI, Docker

**Frontend** (`frontend/`)
- React 19, TypeScript, Vite

## Features

- Sign up, sign in, refresh and sign out, with PBKDF2 password hashing
- Task CRUD with priority, due date and category
- Filtering by status, priority, category, due date and title search, plus sorting and pagination
- Soft delete with a trash list and restore
- Overdue task list and task statistics
- Categories per user
- Real-time events over SignalR: `TaskCreated`, `TaskUpdated`, `TaskDeleted`, `TaskRestored`
- Hourly background job that logs overdue tasks per user
- Global error handling middleware, rate limiting (30 requests/minute), response caching, API versioning (`/api/v1/`) and a health check
- Repository and service layers separating data access from business logic

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) 20.19+ or 22.12+ (frontend only)

### Run the API

```bash
git clone https://github.com/TellSamuelSomething/dotnet-task-api.git
cd dotnet-task-api
dotnet run --launch-profile http
```

The API listens on `http://localhost:5120`. In Development, Swagger is at `http://localhost:5120/swagger` and the Hangfire dashboard at `/hangfire`. The SQLite database (`tasks.db`) is created and migrated automatically on startup.

### Run the frontend

With the API running, in a second terminal:

```bash
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`. The dev server proxies `/api` calls to the API on port 5120.

### Run with Docker

```bash
docker build -t dotnet-task-api .
docker run -p 8080:8080 -e Jwt__Key="a-random-secret-of-at-least-32-characters" dotnet-task-api
```

The container runs in Production mode, so Swagger and the Hangfire dashboard are off, and the SQLite database lives inside the container.

### JWT signing key

No secret is stored in the repo. In Development the API generates a throwaway signing key on every start, so signing in again is needed after a restart (the frontend does this automatically through the refresh token). In any other environment the API refuses to start unless `Jwt__Key` is set.

### Run the tests

```bash
dotnet test TaskAPI.Tests/TaskAPI.Tests.csproj
```

## API overview

All endpoints below are under `/api/v1`. Everything except register, login and refresh requires a `Bearer` token.

### Auth

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/register` | Create an account and receive tokens |
| POST | `/auth/login` | Sign in and receive tokens |
| POST | `/auth/refresh` | Exchange a refresh token for a new token pair |
| POST | `/auth/logout` | Revoke a refresh token |

### Tasks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/tasks` | List tasks with filtering, sorting and pagination |
| GET | `/tasks/{id}` | Get one task |
| POST | `/tasks` | Create a task |
| PUT | `/tasks/{id}` | Update a task |
| DELETE | `/tasks/{id}` | Move a task to the trash (soft delete) |
| POST | `/tasks/{id}/restore` | Restore a task from the trash |
| GET | `/tasks/overdue` | List overdue tasks |
| GET | `/tasks/trash` | List trashed tasks |
| GET | `/tasks/stats` | Completion and priority statistics |

Query parameters for `GET /tasks`: `completed`, `search`, `priority`, `dueBefore`, `categoryId`, `sortBy` (`title`, `dueDate`, `priority`, `createdAt`), `order` (`asc` or `desc`), `page` and `pageSize`.

### Categories

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/categories` | List your categories |
| POST | `/categories` | Create a category |
| DELETE | `/categories/{id}` | Delete a category |

### Other

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Health check, including the database |
| `/hubs/tasks` | SignalR hub for real-time task events (pass the token as `access_token`) |

## Usage example

```bash
# Register (returns an access token and a refresh token)
curl -X POST http://localhost:5120/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username": "demo", "password": "demo1234"}'

# Use the access token
curl http://localhost:5120/api/v1/tasks \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```
