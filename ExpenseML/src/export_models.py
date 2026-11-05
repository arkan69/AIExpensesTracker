"""Export v0.6 Indonesian classifiers without modifying baseline artifacts.
Run from ExpenseML: python src/export_models.py
Requires reports/train_split.csv and data/indonesian_expansion.csv.
"""
from __future__ import annotations

import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

import joblib
import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.pipeline import FeatureUnion, Pipeline
from sklearn.linear_model import LogisticRegression
from sklearn.svm import LinearSVC

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / 'data'
REPORTS = ROOT / 'reports'
MODELS = ROOT / 'models'
VERSION = 'v0.6'
INCOME = {'Business', 'Other Income', 'Salary'}


def pipeline(task: str) -> Pipeline:
    features = FeatureUnion([
        ('word', TfidfVectorizer(analyzer='word', ngram_range=(1, 2), sublinear_tf=True)),
        ('char', TfidfVectorizer(analyzer='char', ngram_range=(2, 5), sublinear_tf=True)),
    ])
    classifier = (LogisticRegression(max_iter=2000, class_weight='balanced', random_state=42)
                  if task == 'intent' else
                  LinearSVC(class_weight='balanced', random_state=42, max_iter=5000))
    return Pipeline([('features', features), ('classifier', classifier)])


def main() -> None:
    paths = [REPORTS / 'train_split.csv', DATA / 'indonesian_expansion.csv']
    for path in paths:
        if not path.is_file():
            raise FileNotFoundError(f'Missing {path}. Run the earlier training/expansion scripts first.')
    original = pd.read_csv(paths[0], dtype=str, keep_default_na=False)
    extra = pd.read_csv(paths[1], dtype=str, keep_default_na=False)
    required = {'text', 'intent', 'category', 'type', 'scenario_group'}
    for name, df in [('original training', original), ('expansion', extra)]:
        if not required.issubset(df.columns):
            raise ValueError(f'{name} missing columns: {required - set(df.columns)}')
        if df['text'].str.strip().eq('').any():
            raise ValueError(f'{name} contains empty text')
    combined = pd.concat([original, extra], ignore_index=True)
    if combined['text'].str.strip().str.casefold().duplicated().any():
        raise ValueError('Duplicate messages in combined training set')
    for group, block in combined.groupby('scenario_group'):
        if block['intent'].nunique() != 1 or block['category'].nunique() != 1:
            raise ValueError(f'Conflicting labels in scenario group {group}')
    transactions = combined[combined['intent'] == 'CreateTransaction'].copy()
    if transactions.empty or transactions['category'].eq('').any():
        raise ValueError('Missing transaction category')
    for _, row in transactions.iterrows():
        expected = 'Income' if row['category'] in INCOME else 'Expense'
        if row['type'] != expected:
            raise ValueError(f'Category/type mismatch: {row["category"]} / {row["type"]}')
    models = {}
    for task, frame, target in [('intent', combined, 'intent'), ('category', transactions, 'category')]:
        model = pipeline(task)
        model.fit(frame['text'], frame[target])
        models[task] = model
        print(f'{task}: trained on {len(frame)} examples; classes={len(model.classes_)}')
    MODELS.mkdir(parents=True, exist_ok=True)
    metadata = {
        'version': VERSION,
        'created_utc': datetime.now(timezone.utc).isoformat(),
        'language': 'id',
        'training_rows': len(combined),
        'transaction_rows': len(transactions),
        'training_sources': [str(p.relative_to(ROOT)) for p in paths],
        'training_sha256': {str(p.relative_to(ROOT)): hashlib.sha256(p.read_bytes()).hexdigest() for p in paths},
        'intent_classes': list(models['intent'].classes_),
        'category_classes': list(models['category'].classes_),
        'category_type': {name: ('Income' if name in INCOME else 'Expense') for name in models['category'].classes_},
        'intent_score': 'predict_proba (model probability estimate; not externally calibrated)',
        'category_score': 'LinearSVC decision_function margin (NOT a probability)',
        'safety': 'prototype only; requires explicit confirmation before saving transactions',
        'evaluation': 'v0.5 synthetic challenge only; models refitted on development training rows',
    }
    # Refuse to overwrite existing versioned artifacts without explicit removal.
    targets = [MODELS / f'{task}_{VERSION}.joblib' for task in models] + [MODELS / f'metadata_{VERSION}.json']
    existing = [str(p) for p in targets if p.exists()]
    if existing:
        raise FileExistsError('Versioned artifacts already exist; refusing to overwrite: ' + ', '.join(existing))
    for task, model in models.items():
        joblib.dump(model, MODELS / f'{task}_{VERSION}.joblib')
    targets[-1].write_text(json.dumps(metadata, ensure_ascii=False, indent=2), encoding='utf-8')
    print('Saved:', ', '.join(str(p.relative_to(ROOT)) for p in targets))
    print('Baseline models and held-out test data were not modified.')


if __name__ == '__main__':
    main()
