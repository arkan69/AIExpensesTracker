import re
from datetime import date, timedelta

MONTHS = {
    "januari": 1, "jan": 1,
    "februari": 2, "feb": 2,
    "maret": 3, "mar": 3,
    "april": 4, "apr": 4,
    "mei": 5,
    "juni": 6, "jun": 6,
    "juli": 7, "jul": 7,
    "agustus": 8, "agu": 8, "agt": 8,
    "september": 9, "sep": 9,
    "oktober": 10, "okt": 10,
    "november": 11, "nov": 11,
    "desember": 12, "des": 12,
}

MONTH_PATTERN = "|".join(
    sorted(MONTHS, key=len, reverse=True)
)

EXPLICIT_DATE = re.compile(
    rf"\b(?:tanggal\s+|tgl\.?\s+)?"
    rf"(\d{{1,2}})\s+({MONTH_PATTERN})"
    rf"(?:\s+(\d{{4}}))?\b",
    re.IGNORECASE,
)

DAY_ONLY = re.compile(
    r"\b(?:tanggal|tgl\.?)\s*(\d{1,2})\b",
    re.IGNORECASE,
)

RELATIVE_DATES = [
    (re.compile(r"\b(?:kemarin lusa|kemaren lusa)\b", re.I), 2),
    (re.compile(r"\b(?:kemarin|kemaren)\b", re.I), 1),
    (re.compile(r"\b(?:hari ini|sekarang)\b", re.I), 0),
]


def parse_date(text: str, reference_date: date | None = None) -> dict:
    today = reference_date or date.today()

    explicit = EXPLICIT_DATE.search(text)

    if explicit:
        day = int(explicit.group(1))
        month = MONTHS[explicit.group(2).lower()]
        year_text = explicit.group(3)

        if year_text:
            year = int(year_text)
        else:
            # Prefer the most recent occurrence, not a future date.
            year = today.year

            if (month, day) > (today.month, today.day):
                year -= 1

        try:
            parsed = date(year, month, day)
        except ValueError:
            return {"status": "INVALID_DATE", "date": None}

        return {
            "status": "OK",
            "date": parsed.isoformat(),
            "source": "EXPLICIT",
        }

    if DAY_ONLY.search(text):
        return {
            "status": "DATE_AMBIGUOUS",
            "date": None,
        }

    for pattern, days_ago in RELATIVE_DATES:
        if pattern.search(text):
            parsed = today - timedelta(days=days_ago)

            return {
                "status": "OK",
                "date": parsed.isoformat(),
                "source": "RELATIVE",
            }

    return {
        "status": "DATE_REQUIRED",
        "date": None,
    }
