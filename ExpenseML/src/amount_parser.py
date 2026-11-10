import re
from decimal import Decimal, InvalidOperation

AMOUNT_PATTERN = re.compile(
    r"(?<![\w.,])(?:"
    r"(?P<rupiah>rp\.?\s*)?"
    r"(?P<number>\d+(?:[.,]\d+)*)"
    r"\s*(?P<unit>juta|ribu|jt|rb|k)?"
    r")\b",
    re.IGNORECASE,
)

BARE_AMOUNT_PATTERN = re.compile(
    r"(?<![\w.,])\d{4,12}(?![\w.,])"
)

MULTIPLIERS = {
    "juta": Decimal("1000000"),
    "jt": Decimal("1000000"),
    "ribu": Decimal("1000"),
    "rb": Decimal("1000"),
    "k": Decimal("1000"),
}


def parse_amount_value(number: str, unit: str | None) -> int | None:
    try:
        if unit:
            normalized = number.replace(",", ".")
            value = Decimal(normalized) * MULTIPLIERS[unit.lower()]
        else:
            normalized = number.replace(".", "").replace(",", "")
            value = Decimal(normalized)

        if value <= 0 or value != value.to_integral_value():
            return None

        return int(value)

    except (InvalidOperation, KeyError):
        return None


def is_non_monetary_number(text: str, start: int, end: int) -> bool:
    number = text[start:end]
    prefix = text[:start].lower()

    # Ignore years when they appear in date/year context.
    if len(number) == 4 and 1900 <= int(number) <= 2100:
        if re.search(
            r"(?:\btahun\s+|"
            r"\b(?:januari|februari|maret|april|mei|juni|juli|"
            r"agustus|september|oktober|november|desember)\s+)$",
            prefix,
        ):
            return True

    # Ignore account numbers and similar identifiers.
    if re.search(
        r"\b(?:rekening|rek|nomor rekening|no rekening|"
        r"norek|no\.?\s*rek)\s*$",
        prefix,
    ):
        return True

    return False



def extract_amounts(text: str) -> list[int]:
    amounts = []
    matched_spans = []

    # First priority: amounts with explicit monetary context.
    for match in AMOUNT_PATTERN.finditer(text):
        number = match.group("number")
        unit = match.group("unit")
        rupiah = match.group("rupiah")

        if not unit and not rupiah and not re.fullmatch(
            r"\d{1,3}(?:\.\d{3})+", number
        ):
            continue

        amount = parse_amount_value(number, unit)

        if amount is not None:
            amounts.append(amount)
            matched_spans.append(match.span())

    # Second priority: bare numeric amounts, e.g. 15000.
    for match in BARE_AMOUNT_PATTERN.finditer(text):
        start, end = match.span()

        # Ignore years, account numbers, and other non-monetary values.
        if is_non_monetary_number(text, start, end):
            continue

        # Avoid counting a monetary amount twice.
        if any(
            start < span_end and end > span_start
            for span_start, span_end in matched_spans
        ):
            continue

        amount = int(match.group())

        if amount > 0:
            amounts.append(amount)

    return amounts




def parse_amount(text: str) -> dict:
    amounts = extract_amounts(text)

    if not amounts:
        return {"status": "MISSING_AMOUNT", "amount": None}

    if len(amounts) > 1:
        return {"status": "MULTIPLE_AMOUNTS", "amount": None}

    return {"status": "OK", "amount": amounts[0]}
