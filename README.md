# Elevating

Elevating is a full-stack goal management application built with ASP.NET Core and Angular.

It allows users to organize their goals, track progress, manage priorities, and stay focused through a clean and responsive interface.

## Features

- Create, view, edit, complete, and delete goals
- Search goals by title or description
- Filter by category, status, and priority
- Sort by multiple fields
- Paginated results
- Goal status tracking
- Priority indicators
- Form validation
- Confirmation dialogs
- Success and error notifications
- Responsive user interface
- Structured API error handling
- Unit and integration tests

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

## API Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/goals` | Get a paginated list of goals |
| GET | `/api/goals/{id}` | Get a goal by ID |
| POST | `/api/goals` | Create a goal |
| PUT | `/api/goals/{id}` | Update a goal |
| PATCH | `/api/goals/{id}/complete` | Mark a goal as completed |
| DELETE | `/api/goals/{id}` | Delete a goal |
| GET | `/api/health` | Check the API status |

## Getting Started

### Prerequisites

Make sure the following tools are installed:

- .NET SDK
- Node.js
- npm
- SQL Server

### Clone the Repository

```powershell
git clone https://github.com/AurelieBR/Elevating.git
cd Elevating
```

### Run the API

Navigate to the API project:

```powershell
cd src/Elevating.Api
```

Apply the database migrations:

```powershell
dotnet ef database update
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
cd src/Elevating.Web
```

Install the dependencies:

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

### Frontend Build

From the Angular project:

```powershell
npm run build
```

## Design

Elevating uses a calm, earthy visual identity designed to feel motivating, elegant, and professional.

The interface includes:

- Responsive goal cards
- Status and priority indicators
- Accessible forms and controls
- Loading, empty, and error states
- Confirmation dialogs
- Muted animations and transitions
- Dashboard statistics & Overdue tracking

## Roadmap

Possible future improvements include:

- User authentication
- User-specific goals
- Goal progress percentages
- Goal notes and milestones
- Cloud deployment
- Continuous integration and deployment
- Improved accessibility testing
