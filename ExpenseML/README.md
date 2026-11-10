# ExpenseML — Indonesian Expense Message Prototype

[← Project overview](../README.md)

`ExpenseML` is a local Python ML and parsing prototype for interpreting Indonesian personal-finance messages. It currently **does not connect to ExpenseApi or save transactions**.

## Current version

**v0.7** adds amount and date extraction to the existing **v0.6** intent/category models. Checkpoint commit: `597e1f0` (`development`). The latest reported regression run passed **8 unittest methods covering 33 scenarios**.

## Capabilities

- **Intent model:** `CreateTransaction`, `SummaryQuery`, `BudgetQuery`, `Other`
- **Category model:** 11 categories (8 expense and 3 income) from v0.6
- **Amount parser:** supports `15000`, `25rb`, `1,5jt`, `Rp150000`, and common thousand separators
- **Date parser:** supports `hari ini`, `kemarin`, `kemarin lusa`, and dates such as `5 November 2026`
- **Transaction parser:** combines extraction results and identifies missing/ambiguous information
- **Safety:** messages involving cancellation/negation require review; all transaction drafts require confirmation

Examples:

| Input | Expected handling |
| --- | --- |
| `kemarin beli nasi padang 25rb` | Draft with amount and relative date; needs confirmation |
| `beli kopi 20rb` | Amount parsed; missing date requires review |
| `kemarin beli nasi 25rb dan kopi 15rb` | Multiple amounts; requires review |
| `belum jadi beli tiket ya` | Safety review; do not create transaction |
| `berapa pengeluaran bulan ini?` | Summary intent classification, not a transaction |

## Source layout

```text
ExpenseML/
├── src/
│   ├── predict.py
│   ├── amount_parser.py
│   ├── date_parser.py
│   └── transaction_parser.py
├── tests/
│   ├── test_parsers.py
│   └── test_prediction.py
└── models/
    ├── intent_v0.6.joblib
    ├── category_v0.6.joblib
    └── metadata_v0.6.json
```

The tree lists the relevant v0.7 files and model artifacts, not necessarily every file in this subproject.

## Local setup

From the repository root:

```bash
cd ExpenseML
python3 -m venv .venv
source .venv/bin/activate
```

Install the dependencies specified by the project's existing Python dependency configuration, if present. At minimum the prediction script uses `joblib` and trained scikit-learn models; install compatible versions for the serialized model artifacts. Do not regenerate or overwrite models merely to run the parser tests.

### Run interactive predictions

```bash
python src/predict.py
```

Type Indonesian messages at the prompt; type `exit` to quit. `predict.py` uses v0.6 trained models and v0.7 parsers.

### Run regression tests

```bash
python -m unittest discover -s tests -v
```

Current suite:

| Test file | Coverage |
| --- | --- |
| `tests/test_parsers.py` | 12 amount, 8 date, 8 combined transaction scenarios |
| `tests/test_prediction.py` | 5 integration scenarios using the trained models |

`unittest` reports **8 test methods** because the parser methods use subtests. The reported passing checkpoint covers **33 individual scenarios**.

## Parser behavior and statuses

- Amount: `OK`, `MISSING_AMOUNT`, `MULTIPLE_AMOUNTS`
- Date: `OK`, `DATE_REQUIRED`, `DATE_AMBIGUOUS`, `INVALID_DATE`
- Combined parser: `READY_FOR_CONFIRMATION` or `REVIEW_REQUIRED`
- Prediction pipeline: `NEEDS_CONFIRMATION`, `REVIEW_REQUIRED`, or `CLASSIFIED` depending on intent and validation

Missing dates are **not automatically assigned to today**. Dates without a year use the most recent occurrence under the current parser policy. The `reference_date` argument can make tests deterministic.

## Known limitations

- `beli kopi 20rb` has previously been classified as `Shopping` instead of `Food`; this is tracked for evaluation, not treated as correct training behavior.
- Four-digit numbers in some year contexts (e.g. `pada 2026`) can be confused with amounts.
- Account identifiers and monetary values are disambiguated with limited context rules, not general semantic understanding.
- Multiple amounts in one message trigger review; multi-transaction extraction is not supported yet.
- Some date formats, conversational variants, and cancellation expressions remain outside tested coverage.
- Category SVM margins are **not calibrated probabilities**.
- Passing tests demonstrates covered scenarios only, not production-level accuracy or security.

## Next milestone: v0.8

Build a realistic, labeled Indonesian evaluation dataset; measure category errors and ambiguity cases; improve parsing and classification only when changes can be evaluated against the baseline. Keep transaction confirmation mandatory.

## Safety boundary

ExpenseML produces structured drafts and review flags. It does not persist data, authenticate users, contact Telegram, or invoke ExpenseApi. Future integrations must enforce user identity, business validation, explicit confirmation, and server-side authorization before saving transactions.
