# Personal Finance

A small single-user personal finance manager: record income and expenses, group them into
categories, set a monthly spending limit per category, and see at the end of the month where
the money went.

There is no authentication or authorization — the application is intentionally scoped to a
single user and a small amount of data.

## Features

- **Transactions** — record income and expenses with description, amount, date, and category.
  Server-side filtering by month, category, and type, plus paging.
- **Categories** — create, update, delete, and list income and expense categories.
- **Budgets** — set a spending limit per expense category and month, and compare it against
  what was actually spent. Overruns are flagged with the exact difference
  (for example *Budget €300, spent €310 — over by €10*).
- **Monthly summary** — total income, total expenses, balance, breakdown by category,
  budget versus actual, and the category with the highest spending.

## Technology

| Layer    | Choice |
| -------- | ------ |
| Backend  | ASP.NET Core 10 minimal APIs, Clean Architecture (Domain / Application / Infrastructure / Api) |
| Data     | PostgreSQL 17 with EF Core 10 (Npgsql), migrations applied automatically at startup |
| Frontend | Angular 20 with standalone components |
| Tests    | MSTest with AutoFixture and FakeItEasy |
| Delivery | Docker Compose (postgres + API + Angular behind nginx) |

## Architecture

```
src/
  PersonalFinance.Domain/          Entities and invariants (Category, Transaction, Budget, BudgetMonth)
  PersonalFinance.Application/     Use-case services, DTOs, repository abstractions
  PersonalFinance.Infrastructure/  EF Core DbContext, configurations, repositories, migrations
  PersonalFinance.Api/             Minimal API endpoints, DI, CORS, OpenAPI
  personal-finance-web/            Angular frontend
tests/
  PersonalFinance.Tests/           Unit tests for the budget and summary logic
```

The Domain project has no dependencies. The Application project depends only on Domain and
defines the repository interfaces that Infrastructure implements, so the use-case logic can be
unit tested against fakes without a database.

## Running everything with one command

Requires Docker Desktop.

```bash
docker compose up --build
```

| Service | URL |
| ------- | --- |
| Frontend | http://localhost:8081 |
| API | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| OpenAPI document | http://localhost:8080/openapi/v1.json |
| PostgreSQL | localhost:5432 (`postgres` / `postgres`, database `personalfinance`) |

On the first start the API applies the EF Core migrations and seeds four categories
(*Salary*, *Groceries*, *Rent*, *Leisure*) so the UI is immediately usable.

Stop and remove everything, including the database volume:

```bash
docker compose down -v
```

## Local development

### Database

Only start PostgreSQL from the compose file:

```bash
docker compose up -d db
```

### API

```bash
dotnet run --project src/PersonalFinance.Api
```

The connection string lives in `src/PersonalFinance.Api/appsettings.json` under
`ConnectionStrings:PersonalFinance` and can be overridden with the
`ConnectionStrings__PersonalFinance` environment variable.

CORS origins for the Angular dev server are configured under `Cors:AllowedOrigins`
(default `http://localhost:4200`).

### Frontend

```bash
cd src/personal-finance-web
npm install
npm start
```

The dev server runs on http://localhost:4200 and calls the API on `http://localhost:8080`
directly (allowed by the CORS policy). In the shipped image the app is served by nginx, which
proxies `/api` to the API container, so no CORS is involved there.

### Tests

```bash
dotnet test
```

### Migrations

The EF Core tools are pinned as a local tool:

```bash
dotnet tool restore
dotnet dotnet-ef migrations add <Name> --project src/PersonalFinance.Infrastructure --output-dir Persistence/Migrations
```

## API

All endpoints are documented in Swagger UI. A ready-made request collection for Postman
(also importable into Insomnia and Bruno) is available at
[`docs/PersonalFinance.postman_collection.json`](docs/PersonalFinance.postman_collection.json).
Set the `baseUrl` variable to `http://localhost:8080`.

| Method | Route | Purpose |
| ------ | ----- | ------- |
| GET | `/api/categories` | List categories |
| POST | `/api/categories` | Create a category |
| PUT | `/api/categories/{id}` | Update a category |
| DELETE | `/api/categories/{id}` | Delete an unused category |
| GET | `/api/transactions?year=&month=&categoryId=&type=&page=&pageSize=` | List transactions with filtering and paging |
| POST | `/api/transactions` | Record income or an expense |
| PUT | `/api/transactions/{id}` | Update a transaction |
| DELETE | `/api/transactions/{id}` | Delete a transaction |
| GET | `/api/budgets?year=&month=` | List the budgets of a month |
| GET | `/api/budgets/comparison?year=&month=` | Budget versus actual for a month |
| POST | `/api/budgets` | Set a limit for a category and month |
| PUT | `/api/budgets/{id}` | Change a limit |
| DELETE | `/api/budgets/{id}` | Delete a budget |
| GET | `/api/summary/{year}/{month}` | Monthly overview |
| GET | `/health` | Liveness probe |

Errors are returned as RFC 7807 problem details: `404` for unknown resources, `409` for
conflicts (duplicate budget, deleting a category still in use), and `400` for violated
domain invariants.

## Design decisions

- **Amount is always positive.** The direction of a transaction is derived from its category
  type, which removes the possibility of a negative income or a positive expense.
- **Months are a value object.** `BudgetMonth` validates year and month and is persisted as a
  single sortable `yyyyMM` integer, which keeps the unique `(category, month)` budget index simple.
- **Budgets are expense-only and single-month**, matching the task description; a unique index
  prevents duplicates per category and month.
- **Aggregation happens in the database.** The monthly totals per category are a single
  `GROUP BY` query, not an in-memory aggregation over all transactions.
- **Categories in use cannot be deleted** — the foreign keys use `RESTRICT` and the API returns
  `409 Conflict` with an explanatory message.
