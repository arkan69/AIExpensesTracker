"""Train baseline Indonesian financial-message intent and category classifiers.

Run from anywhere: python src/train.py
Requires: pandas, scikit-learn, joblib
"""
from __future__ import annotations

import json
from pathlib import Path

import joblib
import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score, classification_report, confusion_matrix
from sklearn.model_selection import train_test_split
from sklearn.pipeline import Pipeline

ROOT = Path(__file__).resolve().parents[1]
DATA_PATH = ROOT / "data" / "financial_messages.csv"
MODEL_DIR = ROOT / "models"
REPORT_DIR = ROOT / "reports"
RANDOM_STATE = 42
TEST_SIZE = 0.20


def build_pipeline() -> Pipeline:
    return Pipeline([
        ("tfidf", TfidfVectorizer(analyzer="char", ngram_range=(2, 5), sublinear_tf=True)),
        ("classifier", LogisticRegression(max_iter=2000, class_weight="balanced", random_state=RANDOM_STATE)),
    ])


def evaluate(name: str, model: Pipeline, test: pd.DataFrame, label_col: str) -> dict:
    truth = test[label_col].tolist()
    predicted = model.predict(test["text"].tolist()).tolist()
    labels = sorted(set(truth) | set(predicted))
    report = classification_report(truth, predicted, labels=labels, output_dict=True, zero_division=0)
    results = {
        "model": name,
        "test_examples": len(test),
        "accuracy": accuracy_score(truth, predicted),
        "macro_f1": report["macro avg"]["f1-score"],
        "labels": labels,
        "confusion_matrix": confusion_matrix(truth, predicted, labels=labels).tolist(),
        "classification_report": report,
    }
    print(f"\n{name}: accuracy={results['accuracy']:.3f}, macro-F1={results['macro_f1']:.3f}, test={len(test)}")
    print(classification_report(truth, predicted, labels=labels, zero_division=0))
    return results


def main() -> None:
    if not DATA_PATH.is_file():
        raise FileNotFoundError(f"Dataset missing: {DATA_PATH}")

    df = pd.read_csv(DATA_PATH, dtype=str, keep_default_na=False)
    required = {"text", "intent", "type", "category", "scenario_group"}
    missing = required - set(df.columns)
    if missing:
        raise ValueError(f"Missing columns: {sorted(missing)}")
    if df.empty or df["text"].str.strip().eq("").any():
        raise ValueError("Dataset is empty or contains blank messages")
    if df["text"].duplicated().any():
        raise ValueError("Duplicate message text found")

    # Each scenario_group must have exactly one intent and one category.
    for label in ("intent", "category"):
        if df.groupby("scenario_group")[label].nunique().gt(1).any():
            raise ValueError(f"Some scenario groups contain multiple {label} labels")

    # Stratify scenario groups by intent/category, keeping paraphrases together.
    groups = df.groupby("scenario_group", as_index=False).agg(
        intent=("intent", "first"), category=("category", "first")
    )
    groups["stratum"] = groups.apply(
        lambda r: r["category"] if r["intent"] == "CreateTransaction" else r["intent"], axis=1
    )
    if groups["stratum"].value_counts().min() < 2:
        raise ValueError("Each class needs at least two scenario groups for a stratified split")
    train_groups, test_groups = train_test_split(
        groups["scenario_group"], test_size=TEST_SIZE, random_state=RANDOM_STATE,
        stratify=groups["stratum"]
    )
    train = df[df["scenario_group"].isin(train_groups)].copy()
    test = df[df["scenario_group"].isin(test_groups)].copy()
    assert set(train["scenario_group"]).isdisjoint(test["scenario_group"])

    MODEL_DIR.mkdir(parents=True, exist_ok=True)
    REPORT_DIR.mkdir(parents=True, exist_ok=True)
    train.to_csv(REPORT_DIR / "train_split.csv", index=False)
    test.to_csv(REPORT_DIR / "test_split.csv", index=False)
    print(f"Dataset: {len(df)} rows; training: {len(train)}; held-out: {len(test)}")

    intent_model = build_pipeline()
    intent_model.fit(train["text"], train["intent"])
    joblib.dump(intent_model, MODEL_DIR / "intent_model.joblib")
    intent_results = evaluate("intent", intent_model, test, "intent")

    train_tx = train[train["intent"] == "CreateTransaction"]
    test_tx = test[test["intent"] == "CreateTransaction"]
    category_model = build_pipeline()
    category_model.fit(train_tx["text"], train_tx["category"])
    joblib.dump(category_model, MODEL_DIR / "category_model.joblib")
    category_results = evaluate("category", category_model, test_tx, "category")

    summary = {
        "note": "Synthetic held-out evaluation only; independently authored test set still required.",
        "random_state": RANDOM_STATE,
        "test_size": TEST_SIZE,
        "intent": intent_results,
        "category": category_results,
    }
    (REPORT_DIR / "baseline_metrics.json").write_text(
        json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    print(f"\nSaved models to {MODEL_DIR} and reports to {REPORT_DIR}")


if __name__ == "__main__":
    main()
