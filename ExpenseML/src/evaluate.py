"""Analyze saved ExpenseML v0.1 classifiers without retraining or changing splits.

Run from ExpenseML: python src/evaluate.py
Only load .joblib files you created/trust: joblib uses pickle serialization.
"""
from pathlib import Path
import json

import joblib
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import pandas as pd
from sklearn.metrics import classification_report, confusion_matrix, ConfusionMatrixDisplay, accuracy_score

ROOT = Path(__file__).resolve().parents[1]
REPORTS = ROOT / 'reports'
MODELS = ROOT / 'models'


def analyze(name: str, frame: pd.DataFrame, target: str) -> dict:
    path = MODELS / f'{name}_model.joblib'
    if not path.is_file():
        raise FileNotFoundError(f'Missing model: {path}. Run train.py first.')
    model = joblib.load(path)  # Only load locally generated, trusted artifacts.
    y_true = frame[target].astype(str).tolist()
    y_pred = model.predict(frame['text'].astype(str).tolist()).tolist()
    labels = sorted(set(y_true) | set(y_pred))
    report = classification_report(y_true, y_pred, labels=labels, output_dict=True, zero_division=0)
    print(f'\n{name.upper()} | examples={len(frame)} | accuracy={accuracy_score(y_true, y_pred):.3f} | macro-F1={report["macro avg"]["f1-score"]:.3f}')
    print(classification_report(y_true, y_pred, labels=labels, zero_division=0))

    errors = frame.copy()
    errors['predicted_' + target] = y_pred
    errors = errors[errors[target] != errors['predicted_' + target]]
    errors.to_csv(REPORTS / f'{name}_errors.csv', index=False, encoding='utf-8-sig')
    print(f'Misclassified messages: {len(errors)} (reports/{name}_errors.csv)')
    if not errors.empty:
        print(errors[['text', target, 'predicted_' + target]].head(15).to_string(index=False))

    matrix = confusion_matrix(y_true, y_pred, labels=labels)
    pd.DataFrame(matrix, index=labels, columns=labels).to_csv(REPORTS / f'{name}_confusion_matrix.csv')
    fig, ax = plt.subplots(figsize=(max(8, len(labels) * .9), max(6, len(labels) * .8)))
    ConfusionMatrixDisplay(matrix, display_labels=labels).plot(ax=ax, xticks_rotation=45, cmap='Blues', colorbar=False, values_format='d')
    ax.set_title(f'{name.title()} confusion matrix — held-out synthetic data')
    fig.tight_layout()
    fig.savefig(REPORTS / f'{name}_confusion_matrix.png', dpi=160)
    plt.close(fig)
    return {'examples': len(frame), 'accuracy': float(accuracy_score(y_true, y_pred)), 'macro_f1': float(report['macro avg']['f1-score']), 'errors': len(errors), 'classification_report': report}


def main() -> None:
    test_path = REPORTS / 'test_split.csv'
    if not test_path.is_file():
        raise FileNotFoundError(f'Missing held-out split: {test_path}. Run train.py first.')
    test = pd.read_csv(test_path, dtype=str, keep_default_na=False)
    required = {'text', 'intent', 'category', 'scenario_group'}
    if not required.issubset(test.columns):
        raise ValueError(f'Test CSV missing columns: {sorted(required - set(test.columns))}')
    if test.empty:
        raise ValueError('Test split is empty')
    tx = test[test['intent'] == 'CreateTransaction'].copy()
    if tx.empty:
        raise ValueError('No transaction examples in held-out test split')
    results = {
        'intent': analyze('intent', test, 'intent'),
        'category': analyze('category', tx, 'category'),
    }
    (REPORTS / 'error_analysis_metrics.json').write_text(json.dumps(results, indent=2, ensure_ascii=False), encoding='utf-8')
    print('\nDone. Open reports/*_errors.csv and *_confusion_matrix.png to inspect errors.')
    print('Test split and saved models were not modified.')


if __name__ == '__main__':
    main()
