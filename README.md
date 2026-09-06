# Prisma

Prisma is an AI-enabled educational management backend built with .NET 10 and ASP.NET Core. It provides APIs for authentication, users, lessons, assessments, assignments, payments, storage, and AI-assisted learning workflows.

## Features

- Role-based access for students, teachers, assistants, and administrators.
- JWT authentication with HTTP-only cookie support and policy-based authorization.
- Lesson, section, material, transcript, and student-progress management.
- Mux video integration and S3-compatible object storage.
- Quizzes with multiple-choice, true/false, and written questions.
- AI-assisted written-answer grading, PDF question extraction, RAG chat, lesson summaries, and weekly student reports.
- Assignments, submissions, grades, redeem codes, and Paymob card/Fawry payments.
- Hangfire background jobs, including weekly report generation.
- English (`en-US`) and Arabic (`ar-EG`) localization.
- Serilog structured logging, output caching, OpenAPI, health checks, and Docker support.

## Architecture

The solution follows Clean Architecture and separates the web layer, use cases, domain rules, and infrastructure integrations.

```text
+--------------------------------------------------+
|                    Prisma.API                    |
|        Presentation / HTTP and Web Layer         |
|   Controllers, middleware, filters, OpenAPI      |
+--------------------------+-----------------------+
						   |
+--------------------------v-----------------------+
|                Prisma.Application                |
|              Use Cases / Business Logic          |
|    MediatR commands, queries, validation, DTOs   |
+--------------------------+-----------------------+
						   |
+--------------------------v-----------------------+
|                  Prisma.Domain                   |
|               Enterprise Business Rules          |
|     Entities, value objects, interfaces, rules   |
+--------------------------^-----------------------+
						   |
+--------------------------+-----------------------+
|             Prisma.Infrastructure                |
|       Persistence and External Integrations      |
| EF Core, Identity, Hangfire, AI, storage, email  |
+--------------------------------------------------+
```

| Project | Responsibility |
| --- | --- |
| `Prisma.API` | ASP.NET Core API, controllers, middleware, filters, authentication, health endpoints, and OpenAPI UI. |
| `Prisma.Application` | Application use cases, MediatR commands and queries, validation, and application contracts. |
| `Prisma.Domain` | Entities, value objects, enums, errors, interfaces, repositories, and specifications. |
| `Prisma.Infrastructure` | EF Core persistence, Identity, Hangfire, caching, storage, payments, video, email, and AI integrations. |
| `Prisma.Application.Tests` | Unit tests for application behavior. |
| `Prisma.Integration.Tests` | API and persistence integration tests using ASP.NET Core testing and Testcontainers PostgreSQL. |

## Technology

- .NET 10, ASP.NET Core, Entity Framework Core, and PostgreSQL with pgvector.
- MediatR, FluentValidation, Ardalis Specification, and Microsoft Identity.
- JWT Bearer authentication, HTTP-only cookies, and policy authorization.
- Hangfire with PostgreSQL storage and Valkey/Redis-compatible caching.
- Microsoft Agent Framework, OpenAI, Groq, Semantic Kernel, and vector search.
- Mux for video, AWS S3-compatible APIs for object storage, MailKit for email, and Paymob for payments.
- Serilog for logging and OpenAPI/Swagger for API documentation.

## Solution Structure

```text
Prisma/
├── Prisma.API/                 # Web API and HTTP presentation layer
├── Prisma.Application/         # Use cases and application contracts
├── Prisma.Domain/              # Core domain model
├── Prisma.Infrastructure/     # Persistence and external integrations
├── Prisma.Application.Tests/   # Unit tests
├── Prisma.Integration.Tests/   # Integration tests
├── docker-compose.yml          # Production-like local services
├── docker-compose.override.yml # Development ports and MinIO
└── postman/                    # API collections and environments
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Credentials for the external services enabled in your environment: OpenAI, Groq, Mux, Paymob, SMTP, and object storage.

## Configuration

Do not commit secrets. Local configuration can be supplied through .NET user secrets, environment variables, or a local `.env` file used by Docker Compose. The main configuration sections are:

| Section | Purpose |
| --- | --- |
| `ConnectionStrings` | PostgreSQL and Valkey connections. |
| `JwtSettings` and `IdentitySeed` | Authentication and development admin seed data. |
| `OpenAI` and `Groq` | Models and API credentials for AI features. |
| `Mux` | Video access and signing credentials. |
| `ObjectStorage` and `VideoStorage` | S3-compatible storage configuration. |
| `PaymobSettings` | Payment gateway credentials, integrations, and callback URLs. |
| `EmailSettings` | SMTP configuration. |
| `FeatureManagement` | AI grading, RAG chat, and weekly report feature flags. |

## Run Locally

Start the local infrastructure first:

```bash
docker compose up -d
```

The Compose stack includes:

- PostgreSQL with the pgvector extension (`db`)
- Valkey cache (`cache`)
- PgBouncer connection pooling (`connection-pooler`)
- MinIO S3-compatible object storage in the development override

Then restore and run the API:

```bash
dotnet restore
dotnet run --project Prisma.API
```

The development launch profiles expose the API at:

- HTTP: `http://localhost:5117`
- HTTPS: `https://localhost:7109`
- OpenAPI/Swagger UI: `/swagger`

Application data seeding runs during startup. The API also exposes `/health/live` for liveness and `/health/ready` for readiness checks. The Hangfire dashboard is available at `/hangfire`.

## API

All controllers use the `/api/v1/{controller}` route pattern. The main endpoint groups are:

| Area | Controllers |
| --- | --- |
| Authentication and users | `AuthController`, `UsersController`, `PreferencesController` |
| Student and teacher workflows | `StudentsController`, `TeachersController`, `TeacherStudentsController` |
| Administration | `AdminController`, `PlatformConfigurationsController` |
| Learning content | `LessonsController`, `SectionProgressController`, `LandingPageController` |
| Assessments | `TeacherQuizzesController`, `StudentQuizzesController`, `GradingController`, `GradesController` |
| Assignments | `TeacherAssignmentsController` |
| AI | `AssistantsController`, `RagController` |
| Payments and access | `PaymentsController`, `CodesController` |
| Files and video | `StorageController`, `VideoStorageController` |

OpenAPI and Swagger are enabled in the Development environment.

## Database and Migrations

The application uses PostgreSQL through Entity Framework Core. Database initialization and application data seeding are performed at startup. When migrations are added, use:

```bash
dotnet ef migrations add <MigrationName> \
	--project Prisma.Infrastructure \
	--startup-project Prisma.API

dotnet ef database update \
	--project Prisma.Infrastructure \
	--startup-project Prisma.API
```

## Testing

```bash
dotnet test Prisma.Application.Tests
dotnet test Prisma.Integration.Tests
```

To build the complete solution:

```bash
dotnet build Prisma.sln
```

## Postman

The `postman/` directory contains collections, environments, flows, globals, mocks, and API specifications for exercising the backend.

## License

This project is published for showcasing purposes only. All rights are reserved. No part of the project may be reproduced, distributed, or used without prior written permission from the authors.
