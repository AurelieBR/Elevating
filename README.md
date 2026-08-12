# Elevating

[![CI](https://github.com/AurelieBR/Elevating/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/AurelieBR/Elevating/actions/workflows/ci.yml)
[![Deploy Frontend](https://github.com/AurelieBR/Elevating/actions/workflows/deploy-frontend.yml/badge.svg?branch=main)](https://github.com/AurelieBR/Elevating/actions/workflows/deploy-frontend.yml)

Elevating is a full-stack goal management application built with ASP.NET Core and Angular.

It helps users turn goals into clear, actionable steps, track progress automatically, manage priorities and deadlines, and stay focused through a calm, responsive interface.

## Live Application

Frontend:

```text
https://victorious-bay-085dce910.7.azurestaticapps.net/
```

API:

```text
https://ca-elevating-api-prod.proudground-8d9a69fa.canadacentral.azurecontainerapps.io
```

Health check:

```text
https://ca-elevating-api-prod.proudground-8d9a69fa.canadacentral.azurecontainerapps.io/api/health
```

> The current production version is intentionally anonymous. Authentication and user-specific data are planned as the next major application phase.

## Features

- Create, view, edit, complete, and delete goals
- Break goals into actionable steps
- Add, edit, complete, reopen, skip, and delete goal actions
- Calculate goal progress automatically from completed actions
- Give all actions equal weight in progress calculations
- Automatically complete a goal when all required actions are resolved
- Complete or skip remaining actions when finishing a goal
- Search goals by title or description
- Filter by category, status, priority, and overdue state
- Sort goals by multiple fields
- Paginated goal results
- Dashboard summary with total, not started, in progress, completed, and overdue counts
- Progress indicators on goal cards and goal details
- Overdue goal highlighting
- Completed-goal visual states
- Priority and status indicators
- Form validation
- Confirmation dialogs
- Success and error notifications
- Loading, empty, and error states
- Responsive user interface
- Structured API error handling
- Backend unit and integration tests
- Frontend component tests
- Automated CI quality gates
- Automated frontend deployment

## Tech Stack

### Backend

- ASP.NET Core 10
- C#
- Entity Framework Core
- Azure SQL Database
- FluentValidation
- Swagger / OpenAPI
- xUnit
- Docker

### Frontend

- Angular
- TypeScript
- Standalone components
- Angular Signals
- Reactive Forms
- Tailwind CSS
- Vitest

### Cloud & DevOps

- GitHub Actions
- GitHub Container Registry
- Azure Static Web Apps
- Azure Container Apps
- Azure SQL Database
- Docker
- Environment-specific configuration
- Secret-backed production configuration

## Architecture

### Application architecture

The backend follows a layered architecture:

```text
Elevating.Domain
Elevating.Application
Elevating.Infrastructure
Elevating.Api
```

Responsibilities are separated across the solution:

- **Domain** contains entities and enums.
- **Application** contains DTOs, validation, interfaces, and business logic.
- **Infrastructure** contains Entity Framework Core, repositories, and database configuration.
- **API** exposes HTTP endpoints and request handling.

The Angular application is organized by feature:

```text
src/app/
├── core/
├── shared/
├── layout/
└── features/
    └── goals/
        ├── components/
        ├── models/
        ├── pages/
        └── services/
```

### Production architecture

```text
GitHub
   │
   ├── CI
   │   ├── .NET build and tests
   │   └── Angular formatting, linting, tests, and build
   │
   └── Frontend deployment
          │
          ▼
Azure Static Web Apps
          │
          │ HTTPS
          ▼
Azure Container Apps
ASP.NET Core API
          │
          ▼
Azure SQL Database
```

The API is packaged as a Docker image and published through GitHub Container Registry.

Production configuration is supplied through environment variables and Container App secrets rather than committed configuration files.

## Goal Progress Rules

Goal progress is derived from its actions rather than entered manually.

- All required actions have equal weight.
- Completed actions increase progress automatically.
- Skipped actions remain visible but are excluded from the progress denominator.
- Completing all required actions automatically completes the goal.
- Reopening an action moves a completed goal back to an active state.
- Adding a new action to a completed goal recalculates its progress.
- Goals without actions use a simple binary model:
  - incomplete goal: `0%`
  - completed goal: `100%`

Example:

```text
5 total actions
2 completed
1 skipped
2 pending

Required actions: 4
Progress: 2 / 4 = 50%
```

## API Endpoints

### Goals

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/goals` | Get a paginated, filtered, and sorted list of goals |
| GET | `/api/goals/summary` | Get dashboard goal statistics |
| GET | `/api/goals/{id}` | Get a goal by ID |
| POST | `/api/goals` | Create a goal |
| PUT | `/api/goals/{id}` | Update a goal |
| PATCH | `/api/goals/{id}/complete` | Complete a goal and resolve remaining actions |
| DELETE | `/api/goals/{id}` | Delete a goal |

### Goal Actions

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/goals/{goalId}/actions` | Get all actions for a goal |
| POST | `/api/goals/{goalId}/actions` | Add an action to a goal |
| PUT | `/api/goals/{goalId}/actions/{actionId}` | Update an action |
| PATCH | `/api/goals/{goalId}/actions/{actionId}/complete` | Mark an action as completed |
| PATCH | `/api/goals/{goalId}/actions/{actionId}/reopen` | Reopen a completed or skipped action |
| DELETE | `/api/goals/{goalId}/actions/{actionId}` | Delete an action |

### Health

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/health` | Check the API status |

## Getting Started

### Prerequisites

Install the following tools:

- .NET 10 SDK
- Node.js
- npm
- SQL Server
- Docker Desktop, if building the API container locally

### Clone the Repository

```powershell
git clone https://github.com/AurelieBR/Elevating.git
cd Elevating
```

### Configure the Local Database

Development database configuration belongs in:

```text
src/Elevating.Api/appsettings.Development.json
```

Apply the database migrations from the repository root:

```powershell
dotnet ef database update --project .\src\Elevating.Infrastructure --startup-project .\src\Elevating.Api
```

### Configure Local JWT Signing

The API signs access tokens with an RSA private key and validates them with the matching public key. Generate an ephemeral local key pair in memory and store it in .NET user secrets:

```powershell
$jwtRsa = [System.Security.Cryptography.RSA]::Create()
$jwtRsa.KeySize = 2048
$jwtPrivateKeyPem = $jwtRsa.ExportPkcs8PrivateKeyPem()
$jwtPublicKeyPem = $jwtRsa.ExportSubjectPublicKeyInfoPem()

dotnet user-secrets set "Jwt:PrivateKeyPem" $jwtPrivateKeyPem --project .\src\Elevating.Api
dotnet user-secrets set "Jwt:PublicKeyPem" $jwtPublicKeyPem --project .\src\Elevating.Api

$jwtRsa.Dispose()
```

Do not add either key to an appsettings file or source control. Production should provide `Jwt__PrivateKeyPem` and `Jwt__PublicKeyPem` through Azure Container Apps secret-backed environment variables.

### Run the API

Navigate to the API project:

```powershell
cd .\src\Elevating.Api
```

Start the API using the HTTPS development profile:

```powershell
dotnet watch run --launch-profile https
```

The API will be available at:

```text
https://localhost:7269
```

Swagger will be available at:

```text
https://localhost:7269/swagger
```

### Run the Angular Application

Open another terminal and navigate to the Angular project:

```powershell
cd .\src\Elevating.Web
```

Install dependencies:

```powershell
npm install
```

Start the development server:

```powershell
npm start
```

The application will be available at:

```text
http://localhost:4200
```

Local Angular development uses the `/api` proxy while production uses the Azure Container Apps API endpoint through Angular environment configuration.

## Docker

The ASP.NET Core API uses a multi-stage Docker build:

```text
.NET 10 SDK image
        ↓
Restore and publish
        ↓
ASP.NET Core 10 runtime image
```

Build the image from the repository root:

```powershell
docker build -f .\src\Elevating.Api\Dockerfile -t elevating-api:local .
```

The production container listens on:

```text
8080
```

Production images are published to GitHub Container Registry.

## Testing

### Backend Tests

From the repository root:

```powershell
dotnet test
```

### Frontend Tests

From the Angular project:

```powershell
npm run test:run
```

### Frontend Linting

```powershell
npm run lint
```

### Frontend Production Build

```powershell
npm run build
```

### Complete Frontend Quality Check

```powershell
npm run check
```

This runs formatting validation, linting, frontend tests, and the Angular production build.

## CI/CD

### Continuous Integration

GitHub Actions runs automated quality gates on pushes and pull requests.

The CI workflow validates:

```text
Backend
├── dotnet restore
├── dotnet build --configuration Release
└── dotnet test

Frontend
├── npm ci
└── npm run check
```

The `main` branch is protected and requires the backend and frontend checks to pass before changes can be merged.

### Continuous Deployment

Frontend deployment is handled by:

```text
.github/workflows/deploy-frontend.yml
```

After successful CI on `main`, the Angular application is deployed to Azure Static Web Apps.

The production frontend communicates with the separately hosted ASP.NET Core API through an explicitly configured CORS allowlist.

API container deployment is currently managed separately and can be fully automated in a future deployment workflow.

## Production Configuration

Environment-specific configuration keeps local development and production concerns separate.

### Development

```text
Angular API base URL: /api
Database: Local SQL Server / SQL Express
ASP.NET Core environment: Development
```

### Production

```text
Frontend: Azure Static Web Apps
API: Azure Container Apps
Database: Azure SQL Database
ASP.NET Core environment: Production
```

Sensitive production values such as the Azure SQL connection string are stored as Azure Container App secrets and exposed to ASP.NET Core through secret-backed environment variables.

The production API also uses an explicit frontend CORS origin rather than allowing requests from arbitrary origins.

## Database Deployment

Production database changes are deployed through reviewed EF Core migration scripts rather than automatically migrating the database during API startup.

Generate an idempotent production migration script from the repository root:

```powershell
dotnet ef migrations script --idempotent --project .\src\Elevating.Infrastructure --startup-project .\src\Elevating.Api --output .\deployment\sql\elevating.sql
```

The generated script uses EF Core migration history to apply only missing migrations.

## Design

Elevating uses a calm, earthy visual identity designed to feel motivating, elegant, and professional.

The interface includes:

- Responsive goal cards
- Goal progress bars
- Dashboard statistics
- Overdue goal highlighting
- Completed-goal celebration states
- Clear status and priority indicators
- Accessible forms and controls
- Confirmation dialogs
- Loading, empty, success, and error states
- Muted animations and transitions
- Responsive layouts across desktop and mobile

## Current Project Status

### Complete

- Goal CRUD
- Search, filtering, sorting, and pagination
- Dashboard summary statistics
- Overdue detection and filtering
- Goal actions
- Automatic progress calculation
- Automatic goal status synchronization
- Goal completion with complete-or-skip action resolution
- Responsive Angular interface
- Backend unit and integration tests
- Frontend component tests
- GitHub Actions CI
- Protected `main` quality gates
- Dockerized ASP.NET Core API
- Azure SQL production database
- Azure Container Apps API deployment
- Azure Static Web Apps frontend deployment
- Environment-specific configuration
- Production secret management
- Production CORS configuration
- Automated frontend deployment

## Roadmap

Possible future improvements include:

- User authentication
- User-specific goals and data isolation
- API deployment automation
- Goal notes
- Milestones
- Action reordering
- Action due dates
- Recurring goals
- Reminders and notifications
- Improved accessibility testing
- Expanded dashboard analytics
- Production observability and monitoring
- Key Vault / managed identity hardening
