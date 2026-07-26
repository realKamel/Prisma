# Prisma

**Prisma** is a modern, AI-powered educational management platform built with **.NET 10** following **Clean Architecture** principles. It serves as a comprehensive backend for managing students, teachers, lessons, quizzes, assignments, payments, and more — with deep integration of AI features for grading, content generation, and intelligent assistance.

---

## ✨ Features

### 👥 User Management

- Multi-role system: **Students**, **Teachers**, **Assistants**, and **Admins**
- JWT-based authentication with cookie support
- Role-based authorization with granular permission policies
- User profiles, preferences, and landing page customization

### 📚 Lesson & Content Management

- Lesson catalog with academic year organization
- Support for multiple material types (videos, PDFs, documents)
- Lesson transcripts and AI-powered summarization
- Section management with student progress tracking
- Video streaming via **Mux** integration
- File storage via **S3-compatible object storage** (Backblaze B2)

### 📝 Quiz & Assessment System

- Multiple question types: **MCQ**, **True/False**, **Written**
- Quiz creation and attempt tracking
- AI-powered **written question grading** via agentic workflows
- Extraction of exam content from PDFs using OpenAI
- Grading suggestions with status tracking

### 📊 Assignments & Reports

- Assignment creation and submission management
- **AI-generated weekly student reports**
- Report generation background jobs

### 💳 Payment System

- Integrated with **Paymob** payment gateway
- Supports **Card** and **Fawry** payment methods
- Payment webhook handling and reconciliation
- Redeem code system

### 🤖 AI-Powered Features

- **AI Grading**: Automatic grading of written answers using LLMs
- **RAG Chat**: Retrieval-Augmented Generation chat over lesson content
- **PDF Exam Extraction**: Parse exam PDFs and extract structured questions
- **Lesson Summarization**: Generate summaries from lesson transcripts
- **Report Generation**: AI-crafted weekly student performance reports
- **Groq Integration**: High-speed LLM inference via Groq API
- **Agentic Workflows**: Structured AI workflows for grading and report generation using Microsoft Agents SDK

### 🌐 Additional Features

- **Localization**: Full Arabic (ar-EG) and English (en-US) support
- **Hangfire Dashboard**: Background job management UI at `/hangfire`
- **Health Checks**: Database and service health monitoring at `/health-ui`
- **Serilog Logging**: Structured logging to console, file, and Seq
- **Output Caching**: Configurable response caching policies
- **CORS**: Configured for local development and production (monsterasp.net)
- **OpenAPI / Swagger**: API documentation at `/swagger`
- **Docker Support**: Infrastructure services (PostgreSQL, Seq, Redis) managed via Docker Compose

---

## 🏗️ Architecture

The project follows **Clean Architecture** (layered architecture) principles, ensuring separation of concerns and testability.

```
┌──────────────────────────────────────────────────┐
│                   Prisma.API                     │
│          (Presentation / Web Layer)              │
│     Controllers, Middlewares, Filters, DTOs      │
├──────────────────────────────────────────────────┤
│               Prisma.Application                 │
│            (Use Cases / Business Logic)          │
│    MediatR Commands/Queries, Validation, DTOs    │
├──────────────────────────────────────────────────┤
│              Prisma.Infrastructure               │
│     (Persistence, External Services, Identity)   │
│EF Core, Hangfire, S3, Mux, Paymob, OpenAI, etc.  │
├──────────────────────────────────────────────────┤
│                 Prisma.Domain                    │
│      (Enterprise Business Entities & Rules)      │
│    Entities, Enums, Interfaces, Specifications   │
└──────────────────────────────────────────────────┘
```

### Solution Structure

| Project                    | Description                                                                                  |
| -------------------------- | -------------------------------------------------------------------------------------------- |
| `Prisma.API`               | ASP.NET Core Web API — controllers, middleware, filters, Angular SPA (built into `wwwroot/`) |
| `Prisma.Application`       | Application business logic — MediatR commands/queries, validation, DTOs                      |
| `Prisma.Domain`            | Core domain — entities, enums, interfaces, specifications                                    |
| `Prisma.Infrastructure`    | Data access, external services, identity, background jobs, AI agents                         |
| `Prisma.Application.Tests` | Unit tests for the application layer                                                         |
| `Prisma.Integration.Tests` | Integration tests                                                                            |

---

## 🧱 Tech Stack

| Technology                      | Purpose                                        |
| ------------------------------- | ---------------------------------------------- |
| **.NET 10**                     | Web framework & runtime                        |
| **ASP.NET Core**                | REST API                                       |
| **Entity Framework Core 10**    | ORM & database access                          |
| **PostgreSQL**                  | Primary database (Docker container)            |
| **MediatR**                     | CQRS / command-query separation                |
| **FluentValidation**            | Request validation                             |
| **Ardalis.Specification**       | Specification pattern for queries              |
| **Hangfire**                    | Background job processing (PostgreSQL storage) |
| **Serilog**                     | Structured logging (Console, File, Seq)        |
| **Microsoft Identity**          | User identity & role management                |
| **JWT Bearer**                  | Authentication                                 |
| **Microsoft.Agents.AI**         | Agentic workflows (OpenAI)                     |
| **OpenAI / Groq**               | LLM inference                                  |
| **Mux**                         | Video processing & streaming                   |
| **AWS S3 SDK**                  | Object storage (Backblaze B2)                  |
| **Paymob**                      | Payment gateway (Card & Fawry)                 |
| **MailKit**                     | Email delivery (SMTP)                          |
| **Swagger / OpenAPI**           | API documentation                              |
| **Microsoft.FeatureManagement** | Feature flags                                  |
| **Docker**                      | Containerization                               |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- API keys for: OpenAI, Groq, Mux, Paymob, Backblaze B2 (see `appsettings.json`)

### Clone & Run

```bash
# Clone the repository
git clone https://github.com/realKamel/Prisma.git
cd Prisma

# Start infrastructure services (PostgreSQL, Seq, Redis) via Docker
docker-compose up -d

# Restore dependencies
dotnet restore

# Run the API
dotnet run --project Prisma.API
```

Infrastructure services (PostgreSQL, Seq, Redis) are fully managed by Docker Compose — no manual installation required.

The API will be available at `https://localhost:5001` (or the port configured in `launchSettings.json`).

### Database

Entity Framework Core is used with PostgreSQL. Migrations are located in `Prisma.Infrastructure/Persistence/Migrations/`.

```bash
dotnet ef database update --project Prisma.Infrastructure --startup-project Prisma.API
```

Data seeding runs automatically on application startup (see `UseDataSeedingAsync`).

---

## 🔌 API Endpoints

All API routes follow the pattern: `/api/v1/{controller}`

| Area                 | Controller                  | Description                             |
| -------------------- | --------------------------- | --------------------------------------- |
| **Auth**             | `AuthController`            | Register, login, logout, refresh tokens |
| **Users**            | `UsersController`           | User profile management                 |
| **Students**         | `StudentsController`        | Student-specific operations             |
| **Teachers**         | `TeachersController`        | Teacher-specific operations             |
| **Teacher Students** | `TeacherStudentsController` | Teacher-student relationship management |
| **Admin**            | `AdminController`           | Administrative operations               |
| **Assistant**        | `AssistantController`       | AI-powered assistant chat               |
| **Lessons**          | `LessonsController`         | Lesson CRUD, materials, transcripts     |
| **Sections**         | `SectionsController`        | Section management and progress         |
| **Assignments**      | `AssignmentsController`     | Assignment CRUD and submissions         |
| **Quizzes**          | `QuizzesController`         | Quiz management and attempts            |
| **Grades**           | `GradesController`          | Grade viewing and management            |
| **Payments**         | `PaymentsController`        | Payment processing and history          |
| **Redeem Codes**     | `RedeemCodesController`     | Redeem code management                  |
| **Landing Page**     | `LandingPageController`     | Landing page content                    |
| **Storage**          | `StorageController`         | File upload/download                    |
| **RAG**              | `RAGController`             | AI-powered Q&A over content             |
| **Preferences**      | `PreferencesController`     | User preferences                        |

> **Swagger UI** is available at `/swagger` in development mode.

---

## 🧩 Domain Model

### Core Entities

- **User** (with roles: Student, Teacher, Admin, Assistant)
- **Lesson** — Learning content with materials and transcripts
- **Section** — Course sections with student progress tracking
- **AcademicYear** — Organizational year grouping
- **Quiz** — Assessments with multiple question types
- **QuizAttempt** — Student quiz submissions
- **Question** — Base class (MCQ, True/False, Written)
- **Assignment** — Tasks with student submissions
- **Enrollment** — Student enrollment records
- **Payment** — Transaction records
- **RedeemCode** — Discount/access codes
- **ChatSession** — AI chat sessions
- **AuditLog** — Security & activity auditing

---

## 🔐 Authentication & Authorization

- **JWT tokens** stored in HTTP-only cookies
- Roles: `Student`, `Teacher`, `Admin`, `Assistant`
- Permission-based policy authorization
- Token validation includes issuer, audience, signing key, and lifetime checks

---

## 📦 External Services

### 🐳 Docker-Managed Infrastructure

These services run in Docker containers and are started via `docker-compose up -d`:

| Service        | Usage                     | Notes                      |
| -------------- | ------------------------- | -------------------------- |
| **PostgreSQL** | Primary database          | Hosted in Docker container |
| **Seq**        | Log aggregation           | Structured log viewer      |
| **Redis**      | Caching & data protection | Session & cache storage    |

### ☁️ Cloud Services

These are third-party external services accessed via API keys:

| Service               | Usage                             | Configuration Key                        |
| --------------------- | --------------------------------- | ---------------------------------------- |
| **OpenAI**            | LLM for grading, extraction, chat | `OpenAI:ApiKey`                          |
| **Groq**              | High-speed LLM inference          | `Groq:ApiKey`                            |
| **Mux**               | Video hosting & streaming         | `Mux:TokenId`, `Mux:TokenSecret`         |
| **Backblaze B2** (S3) | File & video storage              | `Storage:AccessKey`, `Storage:SecretKey` |
| **Paymob**            | Payment processing (Card & Fawry) | `PaymobSettings:SecretKey`               |

---

## ☁️ Deployment

The application is deployed as a single unit at:

- **URL**: [https://prisma.runasp.net](https://prisma.runasp.net)

The Angular frontend is built directly into the `Prisma.API/wwwroot/` folder and served as a Single Page Application (SPA) by the ASP.NET Core backend — no separate frontend hosting needed. Both the API and the Angular SPA are hosted together under **monsterasp.net**.

Infrastructure services (PostgreSQL, Seq, Redis) run in Docker containers alongside the application.

---

## 📁 Postman Collection

Postman artifacts are included in the `postman/` directory:

```
postman/
├── collections/     # API request collections
├── environments/    # Environment variables
├── flows/          # Postman Flows
├── globals/        # Workspace globals
├── mocks/          # Mock servers
└── specs/          # API specifications
```

---

## 🧪 Testing

```bash
# Run unit tests
dotnet test Prisma.Application.Tests

# Run integration tests
dotnet test Prisma.Integration.Tests
```

---

## 🛠️ Development

### Useful Commands

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project Prisma.Infrastructure --startup-project Prisma.API

# Apply migrations
dotnet ef database update --project Prisma.Infrastructure --startup-project Prisma.API

# Watch mode (hot reload)
dotnet watch run --project Prisma.API
```

### Feature Flags

Controlled via `FeatureManagement` in `appsettings.json`:

| Flag                         | Description                                  |
| ---------------------------- | -------------------------------------------- |
| `AiGrading`                  | Enable AI-powered grading of written answers |
| `AiRagChat`                  | Enable RAG-based AI chat over lesson content |
| `WeeklyStudentReportUsingAi` | Enable AI-generated weekly reports           |

---

## 📄 License

This project is published **for showcasing purposes only**. All rights are reserved.

No part of this project may be reproduced, distributed, or used in any form without prior written permission from the authors. If you are interested in using this project or any part of it, please contact us to discuss an agreement.
