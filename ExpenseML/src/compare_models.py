"""Compare ML feature/classifier candidates using training-only grouped validation.

Run from ExpenseML: python src/compare_models.py
Does not read test_split.csv or overwrite saved models.
"""
from __future__ import annotations

import json
from pathlib import Path

import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.pipeline import FeatureUnion
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score, classification_report, f1_score
from sklearn.model_selection import train_test_split
from sklearn.pipeline import Pipeline
from sklearn.svm import LinearSVC

ROOT = Path(__file__).resolve().parents[1]
TRAIN_PATH = ROOT / "reports" / "train_split.csv"
REPORT_DIR = ROOT / "reports"
SEED = 42
VALIDATION_FRACTION = 0.25


def features(kind: str):
    char = TfidfVectorizer(analyzer="char", ngram_range=(2, 5), sublinear_tf=True)
    if kind == "char":
        return char
    if kind == "word_char":
        return FeatureUnion([
            ("word", TfidfVectorizer(analyzer="word", ngram_range=(1, 2), sublinear_tf=True)),
            ("char", char),
        ])
    raise ValueError(kind)


def classifier(kind: str):
    if kind == "logreg":
        return LogisticRegression(max_iter=2000, class_weight="balanced", random_state=SEED)
    if kind == "svm":
        return LinearSVC(class_weight="balanced", random_state=SEED, max_iter=5000)
    raise ValueError(kind)


def grouped_validation_split(df: pd.DataFrame):
    # Each group must have one consistent intent and category.
    for col in ("intent", "category"):
        if df.groupby("scenario_group")[col].nunique().gt(1).any():
            raise ValueError(f"Inconsistent {col} within scenario_group")
    groups = df.groupby("scenario_group", as_index=False).agg(
        intent=("intent", "first"), category=("category", "first")
    )
    groups["stratum"] = groups.apply(
        lambda r: r["category"] if r["intent"] == "CreateTransaction" else r["intent"],
        axis=1,
    )
    counts = groups["stratum"].value_counts()
    if counts.min() < 2:
        raise ValueError(f"Not enough groups for stratified validation: {counts.to_dict()}")
    fit_ids, val_ids = train_test_split(
        groups["scenario_group"],
        test_size=VALIDATION_FRACTION,
        random_state=SEED,
        stratify=groups["stratum"],
    )
    fit = df[df["scenario_group"].isin(fit_ids)].copy()
    val = df[df["scenario_group"].isin(val_ids)].copy()
    assert set(fit["scenario_group"]).isdisjoint(val["scenario_group"])
    return fit, val


def main():
    if not TRAIN_PATH.is_file():
        raise FileNotFoundError(f"Missing {TRAIN_PATH}. Run train.py first.")
    df = pd.read_csv(TRAIN_PATH, dtype=str, keep_default_na=False)
    required = {"text", "intent", "category", "scenario_group"}
    if required - set(df.columns):
        raise ValueError(f"Missing columns: {sorted(required - set(df.columns))}")
    if df.empty or df["text"].str.strip().eq("").any():
        raise ValueError("Empty training data or blank text")
    fit, val = grouped_validation_split(df)
    print(f"Training-only comparison: fit={len(fit)}, validation={len(val)}")
    print("Held-out test_split.csv and saved models remain untouched.\n")

    configurations = [
        ("char + LogisticRegression", "char", "logreg"),
        ("char + LinearSVC", "char", "svm"),
        ("word+char + LogisticRegression", "word_char", "logreg"),
        ("word+char + LinearSVC", "word_char", "svm"),
    ]
    results = []
    errors = []
    for task, label in (("intent", "intent"), ("category", "category")):
        task_fit = fit if task == "intent" else fit[fit["intent"] == "CreateTransaction"]
        task_val = val if task == "intent" else val[val["intent"] == "CreateTransaction"]
        if task_fit.empty or task_val.empty:
            raise ValueError(f"Missing {task} examples in fit/validation")
        print(f"=== {task.upper()} (validation={len(task_val)}) ===")
        for name, feature_kind, classifier_kind in configurations:
            model = Pipeline([
                ("tfidf", features(feature_kind)),
                ("classifier", classifier(classifier_kind)),
            ])
            model.fit(task_fit["text"], task_fit[label])
            predicted = model.predict(task_val["text"])
            accuracy = float(accuracy_score(task_val[label], predicted))
            macro_f1 = float(f1_score(task_val[label], predicted, average="macro", zero_division=0))
            results.append({
                "task": task, "configuration": name,
                "accuracy": round(accuracy, 4), "macro_f1": round(macro_f1, 4),
                "fit_examples": len(task_fit), "validation_examples": len(task_val),
                "classification_report": classification_report(
                    task_val[label], predicted, output_dict=True, zero_division=0
                ),
            })
            for message, actual, prediction in zip(task_val["text"], task_val[label], predicted):
                if actual != prediction:
                    errors.append({
                        "task": task, "configuration": name,
                        "text": message, "actual": actual, "predicted": prediction,
                    })
            print(f"{name:<32} accuracy={accuracy:.3f}  macro-F1={macro_f1:.3f}")
        print()

    REPORT_DIR.mkdir(parents=True, exist_ok=True)
    pd.DataFrame([{k: v for k, v in row.items() if k != "classification_report"} for row in results]).to_csv(
        REPORT_DIR / "model_comparison.csv", index=False
    )
    pd.DataFrame(errors, columns=["task", "configuration", "text", "actual", "predicted"]).to_csv(
        REPORT_DIR / "comparison_errors.csv", index=False
    )
    (REPORT_DIR / "model_comparison.json").write_text(
        json.dumps({
            "note": "Exploratory grouped validation within original training split; test set not used.",
            "seed": SEED, "validation_fraction": VALIDATION_FRACTION,
            "results": results,
        }, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    for task in ("intent", "category"):
        best = max((r for r in results if r["task"] == task), key=lambda r: (r["macro_f1"], r["accuracy"]))
        print(f"Best {task} on validation: {best['configuration']} (macro-F1={best['macro_f1']:.3f})")
    print("\nSaved: reports/model_comparison.csv, model_comparison.json, comparison_errors.csv")
    print("No model artifacts or original splits were modified.")


if __name__ == "__main__":
    main()
