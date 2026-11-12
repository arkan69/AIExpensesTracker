import os
import subprocess
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


class TestModelVersion(unittest.TestCase):

    def run_python(self, code, version=None):
        env = os.environ.copy()
        env.pop("EXPENSEML_MODEL_VERSION", None)

        if version is not None:
            env["EXPENSEML_MODEL_VERSION"] = version

        return subprocess.run(
            [sys.executable, "-c", code],
            cwd=ROOT,
            env=env,
            text=True,
            capture_output=True,
        )

    def test_default_model_version(self):
        result = self.run_python(
            "from src.predict import VERSION; print(VERSION)"
        )

        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(result.stdout.strip(), "v0.6")

    def test_version_override(self):
        result = self.run_python(
            "from src.predict import VERSION, load; "
            "_, _, meta = load(); "
            "print(VERSION, meta['version'])",
            version="v0.8.1",
        )

        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(result.stdout.strip(), "v0.8.1 v0.8.1")

    def test_invalid_model_version(self):
        result = self.run_python(
            "from src.predict import load; load()",
            version="v99.invalid",
        )

        self.assertNotEqual(result.returncode, 0)
        self.assertIn("FileNotFoundError", result.stderr)

    def test_confirmation_required_for_both_versions(self):
        code = (
            "from src.predict import load, predict; "
            "intent, category, meta = load(); "
            "result = predict('beli kopi 20rb hari ini', "
            "intent, category, meta); "
            "print(result['status'], result['requiresConfirmation'])"
        )

        for version in ("v0.6", "v0.8.1"):
            with self.subTest(version=version):
                result = self.run_python(code, version)

                self.assertEqual(result.returncode, 0, result.stderr)
                self.assertEqual(
                    result.stdout.strip(),
                    "NEEDS_CONFIRMATION True",
                )


if __name__ == "__main__":
    unittest.main()
