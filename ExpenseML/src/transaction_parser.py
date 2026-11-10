from datetime import date

from src.amount_parser import parse_amount
from src.date_parser import parse_date


def parse_transaction(
    text: str,
    reference_date: date | None = None,
) -> dict:
    amount_result = parse_amount(text)
    date_result = parse_date(text, reference_date)

    issues = []

    if amount_result["status"] != "OK":
        issues.append(amount_result["status"])

    if date_result["status"] != "OK":
        issues.append(date_result["status"])

    return {
        "originalText": text,
        "amount": amount_result["amount"],
        "date": date_result["date"],
        "dateSource": date_result.get("source"),
        "amountStatus": amount_result["status"],
        "dateStatus": date_result["status"],
        "status": "READY_FOR_CONFIRMATION" if not issues else "REVIEW_REQUIRED",
        "issues": issues,
        "requiresConfirmation": True,
    }
