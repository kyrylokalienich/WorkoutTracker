# WorkoutTracker

A full-stack workout tracking app. Users can sign up, create workout plans, schedule sessions, log results, and view progress reports.

## Tech Stack

| Layer    | Technology                        |
|----------|-----------------------------------|
| Frontend | Next.js, MUI, TypeScript          |
| Backend  | ASP.NET Core Web API, EF Core     |
| Database | PostgreSQL                        |
| Auth     | JWT                               |

## Run with Docker

```bash
docker compose up --build
```

| Service  | URL                              |
|----------|----------------------------------|
| Frontend | http://localhost:3000            |
| API      | http://localhost:5000            |
| Swagger  | http://localhost:5000/swagger    |

## Features

- Sign-up / sign-in / token refresh / logout
- Exercise catalog (seeded)
- Workout plans CRUD with exercises
- Session scheduling and completion tracking
- Reports: workout history, progress, muscle groups
