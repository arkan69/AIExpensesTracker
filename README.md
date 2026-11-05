# AI Expense Tracker

A personal expense tracking and Indonesian-language machine learning project built with **.NET 10**, **ASP.NET Core Web API**, **Entity Framework Core**, **SQL Server**, **Python**, and **scikit-learn**.

The project provides multi-user expense tracking, shared categories, monthly budgets, financial summaries, and automatic budget usage/status calculation.

An **Indonesian-language intent and transaction-category classifier (ExpenseML v0.6)** is available as an interactive Python prototype. Amount/date extraction, integration with the .NET API, and chatbot integrations such as **Telegram** (and potentially **WhatsApp**) are planned.

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
- Python 3 and scikit-learn (ExpenseML)
- pandas, NumPy, joblib, and Matplotlib (ML experiments and reports)

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
AIExpensesTracker/                   # Repository root
├── AIExpensesTracker/               # ASP.NET Core API project (ExpenseApi.csproj)
│   ├── Common/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Entities/
│   ├── Migrations/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   └── ExpenseApi.csproj
├── ExpenseML/                       # Indonesian ML prototype
│   ├── data/                        # Training, expansion and challenge datasets
│   ├── models/                      # Saved joblib classifiers and metadata
│   ├── reports/                     # Evaluation results and error analysis
│   ├── src/
│   │   ├── train.py
│   │   ├── evaluate.py
│   │   ├── compare_models.py
│   │   ├── expand_dataset.py
│   │   ├── challenge_test.py
│   │   ├── export_models.py
│   │   └── predict.py
│   └── requirements.txt
├── ExpenseProject.slnx
└── README.md
```

The Python virtual environment (`ExpenseML/.venv/`) is local and should not be committed.

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

## ExpenseML — Indonesian ML Prototype (v0.6)

ExpenseML currently runs **locally in Python**, independently of the .NET API. It uses supervised text classification to predict:

- **Intent:** `CreateTransaction`, `SummaryQuery`, `BudgetQuery`, or `Other`.
- **Transaction category:** one of 11 categories (8 expense categories and 3 income categories), for transaction-intent messages.
- **Transaction type:** `Income` or `Expense`, derived from the predicted category taxonomy.

The initial version focuses on **Bahasa Indonesia**, including common informal transaction expressions and abbreviations. Full English-language support is not a v1 goal.

### ML Approach

- TF-IDF text features combining word and character n-grams.
- Logistic Regression for intent classification.
- Linear SVM (`LinearSVC`) for category classification.
- Group-aware development splitting, model comparison, misclassification reports, and a separate synthetic challenge set.
- Versioned model artifacts (`intent_v0.6.joblib`, `category_v0.6.joblib`) and metadata.

The original dataset has **468 examples**, with **274 additional Indonesian examples** generated separately (742 total available examples before split restrictions). The challenge set contains **128 separately authored synthetic messages**, including 88 transaction examples.

### Experimental Results

| Model | Synthetic challenge accuracy | Macro-F1 | Evaluation examples |
|---|---:|---:|---:|
| Intent (augmented) | 96.9% | 0.950 | 128 |
| Category (augmented) | 86.4% | 0.866 | 88 |

**These are exploratory results on synthetic data, not verified production accuracy.** The classifier can still misinterpret negated or hypothetical transactions. Similarity checks and challenge testing do not replace evaluation on independently collected real-world messages.

### Run ExpenseML Locally

From the repository root:

```bash
cd ExpenseML
python3 -m venv .venv
source .venv/bin/activate     # macOS / Linux
python -m pip install -r requirements.txt
python src/predict.py
```

On Windows, activate with `.venv\Scripts\activate` instead. The interactive predictor expects the v0.6 model artifacts in `ExpenseML/models/`. If they are missing, generate them with:

```bash
python src/export_models.py
```

Example input:

```text
beli nasi padang 25rb
bayar listrik 150rb
berapa pengeluaran bulan ini?
belum jadi beli tiket ya
```

The prototype reports intent, category/type where applicable, and uncertainty or review status. **It does not extract the final amount/date fields, call the Expense API, or save transactions.** Every suggested transaction requires confirmation; suspicious or ambiguous language can be flagged for review. SVM decision margins are **not probabilities**, and displayed intent scores are not externally calibrated.

To inspect or reproduce experiments, see `src/train.py`, `src/evaluate.py`, `src/compare_models.py`, `src/expand_dataset.py`, and `src/challenge_test.py`. Evaluation outputs are under `reports/`.

## Planned AI & Chatbot Integration

The next development phases will add amount/date parsing, API integration, and then chatbot integration. The following diagram is a **planned architecture**, not a currently deployed flow.

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
- [x] Indonesian ML datasets and training pipeline
- [x] Intent and category classifiers (ExpenseML v0.6)
- [x] Synthetic challenge evaluation and error analysis
- [x] Interactive local ML prediction prototype
- [ ] Indonesian amount and date extraction (ExpenseML v0.7)
- [ ] Connect ML inference to the .NET API
- [ ] Messaging account to user mapping
- [ ] Telegram bot integration
- [ ] Bot commands
- [ ] Complete natural-language transaction parser (including amount/date)
- [ ] End-to-end AI integration with the API and bot
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
- Python/scikit-learn model training, evaluation, and deployment planning