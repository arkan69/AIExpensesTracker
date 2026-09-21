# AI Expense Tracker

A backend-focused personal expense tracking application built with **.NET 10**, **ASP.NET Core Web API**, **Entity Framework Core**, and **SQL Server**.

The project provides multi-user expense tracking, shared categories, monthly budgets, financial summaries, and automatic budget usage/status calculation.

The long-term goal is to extend the application with **AI-powered natural-language transaction parsing** and chatbot integrations such as **Telegram** and potentially **WhatsApp**.

## Features

### Users

- Multi-user support
- User roles (`SuperAdmin` and `User`)
- Active/inactive user status
- User-scoped financial data

### Categories

- Global/shared categories
- Income and Expense category types
- Duplicate category protection
- Soft delete using `IsActive`
- Category validation when creating transactions

### Transactions

- Create, read, update, and delete transactions
- User-specific transactions
- Income and Expense transaction types
- Filter transactions by:
  - Year
  - Month
  - Transaction type
- Category validation
- User isolation
- Transaction timestamps

### Budgets

- Monthly budgets per category
- User-specific budgets
- Duplicate budget protection per user/category/month/year
- Expense-category validation
- Budget usage calculation

Budget usage includes:

- Budget limit
- Amount spent
- Remaining amount
- Usage percentage
- Budget status

Budget statuses:

| Usage | Status |
|---|---|
| `< 80%` | Safe |
| `>= 80% and < 100%` | Warning |
| `>= 100%` | Exceeded |

### Monthly Summary

Monthly financial summaries include:

- Total income
- Total expenses
- Balance
- Expenses grouped by category

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- C#
- Entity Framework Core 10
- SQL Server
- Docker
- Swagger / OpenAPI

## Architecture

The backend follows a simple layered architecture:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
AppDbContext
    ↓
SQL Server
```

Responsibilities are separated between layers:

- **Controllers** handle HTTP requests and responses.
- **Services** contain business rules and calculations.
- **Repositories** handle database access.
- **Entity Framework Core** handles ORM and database mapping.
- **DTOs** separate API contracts from database entities.

## Project Structure

```text
ExpenseApi/
├── Common/
│   └── Responses/
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

## API Response Format

API endpoints use a common response structure:

```json
{
  "status": 200,
  "message": "Request completed successfully.",
  "data": {}
}
```

## Main API Routes

### Users

```http
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
```

### Categories

```http
GET    /api/categories
GET    /api/categories/{id}
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}
```

### Transactions

```http
GET    /api/users/{userId}/transactions
GET    /api/users/{userId}/transactions/{id}
GET    /api/users/{userId}/transactions/year/{year}
GET    /api/users/{userId}/transactions/year/{year}/month/{month}
GET    /api/users/{userId}/transactions/type/{type}

POST   /api/users/{userId}/transactions
PUT    /api/users/{userId}/transactions/{id}
DELETE /api/users/{userId}/transactions/{id}
```

### Budgets

```http
GET    /api/users/{userId}/budgets
GET    /api/users/{userId}/budgets/{id}
GET    /api/users/{userId}/budgets/year/{year}
GET    /api/users/{userId}/budgets/year/{year}/month/{month}

POST   /api/users/{userId}/budgets
PUT    /api/users/{userId}/budgets/{id}
DELETE /api/users/{userId}/budgets/{id}
```

### Budget Usage

```http
GET /api/users/{userId}/budgets/year/{year}/month/{month}/usage
```

### Monthly Summary

```http
GET /api/users/{userId}/summary/year/{year}/month/{month}
```

## Local Development

### Requirements

Make sure the following are installed:

- .NET 10 SDK
- Docker
- Git

Check the .NET SDK:

```bash
dotnet --version
```

### Database

The project uses SQL Server running in Docker.

Create or start your SQL Server container and make sure the `AIExpenseTracker` database is available.

### Configuration

Sensitive configuration should **not** be stored in `appsettings.json`.

This project uses .NET User Secrets for local credentials.

Initialize User Secrets:

```bash
dotnet user-secrets init
```

Set the database connection string:

```bash
dotnet user-secrets set \
  "ConnectionStrings:DefaultConnection" \
  "YOUR_CONNECTION_STRING"
```

Do not commit passwords, API keys, bot tokens, or other credentials to Git.

### Database Migration

Apply the existing EF Core migrations:

```bash
dotnet ef database update
```

### Run

Start the API:

```bash
dotnet run
```

For development with hot reload:

```bash
dotnet watch
```

Open the Swagger UI using the localhost URL displayed by ASP.NET Core.

## Planned AI & Chatbot Integration

The next development phase will introduce chatbot integration.

The planned architecture is:

```text
Telegram ─┐
          │
WhatsApp ─┼──> Bot Layer
          │       ↓
          │   AI Parser
          │       ↓
          └──> Expense API
                   ↓
             Business Logic
                   ↓
                Database
```

### Planned Flow

A messaging account will be mapped to an Expense Tracker user.

For example:

```text
Telegram User
      ↓
User Recognition
      ↓
Expense Tracker UserId
      ↓
Command / Natural Language
      ↓
AI Parser
      ↓
Structured Transaction
      ↓
Expense API
```

Eventually, users should be able to write messages such as:

```text
beli makan 15000
```

or:

```text
tadi beli nasi padang 25 ribu
```

and have the AI convert the message into structured transaction information.

Example:

```json
{
  "transactionType": "Expense",
  "amount": 25000,
  "categoryName": "Food",
  "description": "Nasi Padang"
}
```

The transaction will still pass through the existing Expense API business rules before being saved.

After a transaction is recorded, the bot is planned to return relevant information such as:

- Transaction details
- Monthly spending summary
- Budget usage
- Remaining budget
- Safe / Warning / Exceeded status

## Roadmap

- [x] .NET 10 Web API foundation
- [x] SQL Server + EF Core
- [x] Multi-user support
- [x] Global categories
- [x] Transaction management
- [x] Monthly budgets
- [x] Monthly financial summary
- [x] Budget usage calculation
- [x] Budget status / alerts
- [ ] Messaging account to user mapping
- [ ] Telegram bot integration
- [ ] Bot commands
- [ ] Natural-language transaction parser
- [ ] AI integration
- [ ] Transaction confirmation flow
- [ ] Telegram summary and budget responses
- [ ] WhatsApp integration

## Security

Secrets are intentionally kept outside the repository.

Local development credentials should be stored using **.NET User Secrets**.

Future secrets such as:

```text
ConnectionStrings:DefaultConnection
Telegram:BotToken
OpenAI:ApiKey
```

must never be committed to source control.

Authentication and authorization will be implemented in a later development phase.

## Purpose

This is a personal self-learning project focused on improving skills in:

- ASP.NET Core backend development
- REST API design
- Entity Framework Core
- SQL Server
- Multi-user application architecture
- AI integration
- Chatbot integration
- Natural-language processing