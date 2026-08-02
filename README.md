# Elevating

Elevating is a full-stack goal management application built with ASP.NET Core and Angular.

It helps users turn goals into clear, actionable steps, track progress automatically, manage priorities and deadlines, and stay focused through a calm, responsive interface.

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
- Priority and status indicators
- Form validation
- Confirmation dialogs
- Success and error notifications
- Loading, empty, and error states
- Responsive user interface
- Structured API error handling
- Backend unit and integration tests
- Frontend component tests

## Tech Stack

### Backend

- ASP.NET Core
- C#
- Entity Framework Core
- SQL Server
- FluentValidation
- Swagger / OpenAPI
- xUnit

### Frontend

- Angular
- TypeScript
- Standalone components
- Angular Signals
- Reactive Forms
- Tailwind CSS
- Vitest

## Architecture

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

- .NET SDK
- Node.js
- npm
- SQL Server

### Clone the Repository

```powershell
git clone https://github.com/AurelieBR/Elevating.git
cd Elevating
```

### Configure the Database

Update the connection string in the API configuration if needed.

Apply the database migrations from the repository root:

```powershell
dotnet ef database update `
  --project .\src\Elevating.Infrastructure `
  --startup-project .\src\Elevating.Api
```

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

From the Angular project:

```powershell
npm run lint
```

### Frontend Build

From the Angular project:

```powershell
npm run build
```

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

The application currently includes:

- Goal CRUD
- Search, filtering, sorting, and pagination
- Dashboard summary statistics
- Overdue detection and filtering
- Goal actions
- Automatic progress calculation
- Automatic goal status synchronization
- Goal completion with complete-or-skip action resolution
- Responsive Angular interface
- Backend and frontend test coverage

## Roadmap

Possible future improvements include:

- User authentication
- User-specific goals
- Goal notes
- Milestones
- Action reordering
- Action due dates
- Recurring goals
- Reminders and notifications
- Cloud deployment
- Continuous integration and deployment
- Improved accessibility testing
- Expanded dashboard analytics
