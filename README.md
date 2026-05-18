# CI/CD Orchestration System

A lightweight, modular CI/CD orchestrator built with .NET 10. Automates build, test, and deployment pipelines using a controller-agent architecture with message queue dispatch.

## Architecture

```
Server (Orchestrator)  ←── RabbitMQ ──→  Runners (Agents)
     │
     ├── PostgreSQL (state, metadata)
     ├── File storage (logs, artifacts)
     └── Blazor WASM UI (dashboard)
```

## Projects

| Project | Type | Description |
|---|---|---|
| `Orchestrator.Server` | ASP.NET Core API | Central coordinator — webhooks, pipeline engine, job dispatch, API |
| `Orchestrator.Runner` | Console App | Execution agent — pulls jobs, runs steps in Podman containers |
| `Orchestrator.Front` | Blazor WASM | Dashboard — build history, logs, runner status |
| `Orchestrator.Shared` | Class Library | Domain models shared between Server and Front |
| `Orchestrator.Contracts` | Class Library | Message contracts for Server-Runner communication |

## Prerequisites

- .NET 10 SDK
- Docker / Podman
- PostgreSQL 17 (Neon, Supabase, or local)
- RabbitMQ (local or managed)

## Quick Start

```bash
# Clone and restore
git clone <repo-url>
cd cicd-orchestrator
dotnet restore

# Set up database
cp .env.example .env
# Edit .env with your Neon connection string

# Run migrations
dotnet ef database update --project src/Orchestrator.Server

# Start the Server
dotnet run --project src/Orchestrator.Server

# Start a Runner (different terminal)
dotnet run --project src/Orchestrator.Runner

# Trigger a build
curl -X POST http://localhost:5000/api/builds \
  -H "Content-Type: application/json" \
  -d '{"pipelineId":"test","branch":"main"}'

# Open the dashboard
# http://localhost:5000
```

## Deployment

Designed to run on a Raspberry Pi 5 (8GB) or a Hetzner VPS (CAX31, ~$10/mo). The entire stack runs in Docker Compose. See `deployment/` for configuration.

## License

MIT
