# dotnet-task-api

A RESTful Task Management API built with ASP.NET Core 10, demonstrating real-world backend development patterns.

## Tech Stack

- **ASP.NET Core 10** — Web API framework
- **Entity Framework Core** — ORM with SQLite database and migrations
- **JWT Authentication** — Stateless token-based auth
- **xUnit** — Unit and integration tests

## Features

- Full CRUD for tasks (Create, Read, Update, Delete)
- JWT authentication — all task endpoints are protected
- User-scoped tasks — each user only sees their own data
- Filtering by completion status and title search
- Pagination support
- Global error handling middleware
- Rate limiting (30 requests/minute)
- Response caching on GET endpoints
- API versioning (`/api/v1/`)
- Health check endpoint
- Repository pattern for clean data access separation
- Docker support

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run locally

```bash
git clone https://github.com/TellSamuelSomething/dotnet-task-api.git
cd dotnet-task-api
dotnet run
```

Open `https://localhost:{port}/swagger` to explore the API.

### Run with Docker

```bash
docker build -t dotnet-task-api .
docker run -p 8080:8080 dotnet-task-api
```

### Run tests

```bash
dotnet test TaskAPI.Tests/TaskAPI.Tests.csproj
```

## API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/login` | Login and receive a JWT token |

### Tasks (requires Bearer token)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/tasks` | Get all tasks (supports filtering & pagination) |
| GET | `/api/v1/tasks/{id}` | Get a single task |
| POST | `/api/v1/tasks` | Create a task |
| PUT | `/api/v1/tasks/{id}` | Update a task |
| DELETE | `/api/v1/tasks/{id}` | Delete a task |

### Query Parameters for GET /api/v1/tasks
| Parameter | Type | Description |
|-----------|------|-------------|
| `completed` | bool | Filter by completion status |
| `search` | string | Search by title |
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 10) |

### Other
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |

## Usage Example

```bash
# 1. Login
curl -X POST https://localhost:{port}/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password123"}'

# 2. Use the token
curl https://localhost:{port}/api/v1/tasks \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```
