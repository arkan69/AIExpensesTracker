"""Export v0.8 classifiers without overwriting v0.6 artifacts."""
from __future__ import annotations

import hashlib
import json
from datetime import datetime, timezone

import joblib
import pandas as pd

from export_models import (
    ROOT,
    DATA,
    REPORTS,
    MODELS,
    INCOME,
    pipeline,
)

VERSION = "v0.8.1"

PATHS = [
    REPORTS / "train_split.csv",
    DATA / "indonesian_expansion.csv",
    DATA / "indonesian_expansion_v08.csv",
]


def main() -> None:
    frames = []

    required = {"text", "intent", "type", "category", "scenario_group"}

    for path in PATHS:
        if not path.is_file():
            raise FileNotFoundError(f"Missing dataset: {path}")

        frame = pd.read_csv(path, dtype=str, keep_default_na=False)

        if not required.issubset(frame.columns):
            raise ValueError(
                f"{path.name} missing columns: {required - set(frame.columns)}"
            )

        if frame["text"].str.strip().eq("").any():
            raise ValueError(f"Empty messages in {path.name}")

        frames.append(frame)

    combined = pd.concat(frames, ignore_index=True)

    normalized = combined["text"].str.strip().str.casefold()
    if normalized.duplicated().any():
        raise ValueError("Duplicate training messages detected")

    for group, block in combined.groupby("scenario_group"):
        if block["intent"].nunique() != 1 or block["category"].nunique() != 1:
            raise ValueError(f"Conflicting labels in group: {group}")

    transactions = combined[
        combined["intent"] == "CreateTransaction"
    ].copy()

    if transactions.empty or transactions["category"].eq("").any():
        raise ValueError("Missing transaction categories")

    for _, row in transactions.iterrows():
        expected_type = (
            "Income" if row["category"] in INCOME else "Expense"
        )
        if row["type"] != expected_type:
            raise ValueError(
                f"Category/type mismatch: {row['category']} / {row['type']}"
            )

    models = {}

    for task, frame, target in [
        ("intent", combined, "intent"),
        ("category", transactions, "category"),
    ]:
        model = pipeline(task)
        model.fit(frame["text"], frame[target])
        models[task] = model

        print(
            f"{task}: trained on {len(frame)} examples; "
            f"classes={len(model.classes_)}"
        )

    MODELS.mkdir(parents=True, exist_ok=True)

    targets = {
        task: MODELS / f"{task}_{VERSION}.joblib"
        for task in models
    }
    metadata_path = MODELS / f"metadata_{VERSION}.json"

    existing = [
        path for path in [*targets.values(), metadata_path]
        if path.exists()
    ]
    if existing:
        raise FileExistsError(
            "Refusing to overwrite existing artifacts: "
            + ", ".join(map(str, existing))
        )

    metadata = {
        "version": VERSION,
        "created_utc": datetime.now(timezone.utc).isoformat(),
        "language": "id",
        "training_rows": len(combined),
        "transaction_rows": len(transactions),
        "training_sources": [
            str(path.relative_to(ROOT)) for path in PATHS
        ],
        "training_sha256": {
            str(path.relative_to(ROOT)): hashlib.sha256(
                path.read_bytes()
            ).hexdigest()
            for path in PATHS
        },
        "intent_classes": list(models["intent"].classes_),
        "category_classes": list(models["category"].classes_),
        "category_type": {
            name: (
                "Income" if name in INCOME else "Expense"
            )
            for name in models["category"].classes_
        },
        "intent_score": (
            "predict_proba; not externally calibrated"
        ),
        "category_score": (
            "LinearSVC decision_function; not probability"
        ),
        "safety": (
            "Prototype only; explicit confirmation required"
        ),
        "evaluation": (
            "Not yet evaluated; compare against v0.6 "
            "using unchanged challenge data"
        ),
    }

    for task, model in models.items():
        joblib.dump(model, targets[task])

    metadata_path.write_text(
        json.dumps(metadata, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )

    print(f"Saved {VERSION} artifacts successfully.")
    print("Original v0.6 artifacts remain untouched.")


if __name__ == "__main__":
    main()
