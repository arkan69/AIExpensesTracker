"""Interactive Indonesian financial message classifier (no database writes).
Run from ExpenseML: python src/predict.py
"""
from __future__ import annotations

import json
import re
import sys
from datetime import date
from pathlib import Path

import joblib

ROOT = Path(__file__).resolve().parents[1]
# Support both `python src/predict.py` and imports from the ExpenseML root.
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from src.transaction_parser import parse_transaction

MODELS = ROOT / 'models'
VERSION = 'v0.6'

# Conservative safety guard for common negation, cancellation, and hypothetical language.
# It is NOT a comprehensive Indonesian language parser.
REVIEW_PATTERN = re.compile(
    r'\b(?:jangan|batalkan|batal|belum\s+jadi|gak\s+jadi|nggak\s+jadi|tidak\s+jadi|'
    r'ga\s+jadi|cuma\s+cerita|hanya\s+cerita|kalau|misalnya|andaikan|rencana|'
    r'ingin|pengen|mau\s+beli|mau\s+bayar)\b', re.IGNORECASE)


def load():
    meta = json.loads((MODELS / f'metadata_{VERSION}.json').read_text(encoding='utf-8'))
    if meta.get('version') != VERSION:
        raise ValueError('Model metadata version mismatch')
    return (joblib.load(MODELS / f'intent_{VERSION}.joblib'),
            joblib.load(MODELS / f'category_{VERSION}.joblib'), meta)


def predict(message, intent_model, category_model, meta, reference_date: date | None = None):
    intent = str(intent_model.predict([message])[0])
    probabilities = intent_model.predict_proba([message])[0]
    intent_probability = float(max(probabilities))
    needs_review = bool(REVIEW_PATTERN.search(message))
    result = {'message': message, 'intent': intent,
              'intent_probability_estimate': round(intent_probability, 3),
              'status': 'REVIEW_REQUIRED' if needs_review else 'CLASSIFIED',
              'category': None, 'type': None, 'category_margin': None,
              'amount': None, 'date': None, 'dateSource': None,
              'amountStatus': None, 'dateStatus': None,
              'issues': [], 'requiresConfirmation': False}
    if intent == 'CreateTransaction':
        category = str(category_model.predict([message])[0])
        margins = category_model.decision_function([message])[0]
        result['category'] = category
        result['type'] = meta['category_type'][category]
        result['category_margin'] = round(float(max(margins)), 3)
        # Always require confirmation. Category margin is NOT a probability.
        extraction = parse_transaction(message, reference_date)
        for key in ('amount', 'date', 'dateSource', 'amountStatus', 'dateStatus', 'issues'):
            result[key] = extraction[key]
        result['requiresConfirmation'] = True
        if needs_review:
            result['issues'].append('SAFETY_REVIEW')
        result['status'] = (
            'REVIEW_REQUIRED' if result['issues'] else 'NEEDS_CONFIRMATION'
        )
    elif intent_probability < 0.65:
        result['status'] = 'REVIEW_REQUIRED'
    return result


def main():
    intent_model, category_model, meta = load()
    print('ExpenseML v0.7 | Indonesian prototype | type "exit" to quit')
    print('No database operations. No transaction will be saved.\n')
    while True:
        try:
            message = input('You: ').strip()
        except (EOFError, KeyboardInterrupt):
            print('\nBye!')
            break
        if message.lower() in {'exit', 'quit', 'keluar'}:
            break
        if not message:
            continue
        result = predict(message, intent_model, category_model, meta)
        print('Intent   :', result['intent'])
        print('Intent score (uncalibrated):', result['intent_probability_estimate'])
        if result['category']:
            print('Category :', result['category'])
            print('Type     :', result['type'])
            print('SVM margin (not probability):', result['category_margin'])
        if result['intent'] == 'CreateTransaction':
            print('Amount   :', result['amount'])
            print('Date     :', result['date'])
            print('Issues   :', result['issues'])
        print('Status   :', result['status'])
        if result['status'] == 'REVIEW_REQUIRED':
            print('Action   : Ask user to clarify; do not create a transaction')
        elif result['status'] == 'NEEDS_CONFIRMATION':
            print('Action   : Ask user to confirm; no transaction is created')
        print()


if __name__ == '__main__':
    main()
