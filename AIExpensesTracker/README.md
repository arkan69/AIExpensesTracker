# ExpenseApi — .NET Expense Tracking Backend

[← Project overview](../README.md)

`ExpenseApi` is the .NET 10 / ASP.NET Core Web API portion of AI Expense Tracker. It manages user-scoped finance data with SQL Server and Entity Framework Core.

## Features

- **Users:** multi-user records, `SuperAdmin` / `User` role data, active/inactive state
- **Categories:** shared income/expense categories, duplicate protection, soft deletion (`IsActive`)
- **Transactions:** CRUD, per-user isolation, year/month/type filtering, category validation
- **Budgets:** monthly category limits, duplicate protection, usage and remaining amount
- **Budget status:** `Safe` below 80%, `Warning` from 80% to below 100%, `Exceeded` at 100% or more
- **Monthly summary:** total income, expenses, balance, expenses grouped by category
- **Responses:** consistent `status`, `message`, `data` envelope

## Stack and architecture

.NET 10, C#, ASP.NET Core Web API, EF Core 10, SQL Server, Swagger/OpenAPI.

```text
Controller -> Service -> Repository -> AppDbContext -> SQL Server
```

Controllers expose HTTP endpoints; services implement business rules; repositories encapsulate data access; DTOs define API contracts; EF Core maps entities and migrations.

## Project layout

```text
ExpenseApi/
├── Common/Responses/
├── Controllers/
├── Data/
├── DTOs/
│   ├── Budgets/
│   ├── Categories/
│   ├── Summaries/
│   ├── Transactions/
│   └── Users/
├── Entities/
├── Migrations/
├── Repositories/
├── Services/
├── Program.cs
├── ExpenseApi.csproj
└── appsettings.json
```

## API routes

The following routes reflect the previously documented API contract. Check controllers when changing endpoint signatures.

```http
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}

GET    /api/categories
GET    /api/categories/{id}
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}

GET    /api/users/{userId}/transactions
GET    /api/users/{userId}/transactions/{id}
GET    /api/users/{userId}/transactions/year/{year}
GET    /api/users/{userId}/transactions/year/{year}/month/{month}
GET    /api/users/{userId}/transactions/type/{type}
POST   /api/users/{userId}/transactions
PUT    /api/users/{userId}/transactions/{id}
DELETE /api/users/{userId}/transactions/{id}

GET    /api/users/{userId}/budgets
GET    /api/users/{userId}/budgets/{id}
GET    /api/users/{userId}/budgets/year/{year}
GET    /api/users/{userId}/budgets/year/{year}/month/{month}
POST   /api/users/{userId}/budgets
PUT    /api/users/{userId}/budgets/{id}
DELETE /api/users/{userId}/budgets/{id}
GET    /api/users/{userId}/budgets/year/{year}/month/{month}/usage

GET    /api/users/{userId}/summary/year/{year}/month/{month}
```

### Response envelope

```json
{
  "status": 200,
  "message": "Request completed successfully.",
  "data": {}
}
```

## Local development

**Prerequisites:** .NET 10 SDK, SQL Server (local or Docker), Git, and EF Core CLI tools for migrations.

```bash
cd ExpenseApi
dotnet --version
dotnet restore
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
dotnet ef database update
dotnet run
```

For hot reload:

```bash
dotnet watch
```

Use the local URL printed by ASP.NET Core. The OpenAPI document is available at `/openapi/v1.json` in the current project setup; Swagger UI availability depends on local configuration.

**Database note:** SQL Server can run natively on Windows or in Docker. Do not assume a Docker container is running or create one without checking the existing local configuration.

## Integration status

The API currently handles normal backend operations. The separate [ExpenseML prototype](../ExpenseML/README.md) is **not yet connected** to the API. A future integration should validate the proposed transaction, identify the user securely, require confirmation, and then use existing API business rules to persist it.

## Security and planned work

- Keep connection strings, passwords, and tokens out of Git; use .NET User Secrets locally.
- Multi-user IDs and role fields are not substitutes for implemented authentication and authorization.
- Planned: messaging identity mapping, authentication/security, bot integration, deployment, and regression coverage for future AI-assisted transaction creation.
