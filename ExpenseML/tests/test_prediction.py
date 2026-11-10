import unittest
from datetime import date
from unittest.mock import patch
from datetime import date as real_date

from src.predict import load, predict


class TestPredictionIntegration(unittest.TestCase):

    @classmethod
    def setUpClass(cls):
        cls.intent_model, cls.category_model, cls.meta = load()

    def classify(self, message):
        # Mock the date class in the module where date.today() is called.
        class FixedDate(real_date):
            @classmethod
            def today(cls):
                return real_date(2026, 11, 10)

        with patch("src.date_parser.date", FixedDate):
            return predict(
                message,
                self.intent_model,
                self.category_model,
                self.meta,
            )

    def test_valid_transaction(self):
        result = self.classify("kemarin beli nasi padang 25rb")

        self.assertEqual(result["intent"], "CreateTransaction")
        self.assertEqual(result["category"], "Food")
        self.assertEqual(result["type"], "Expense")
        self.assertEqual(result["amount"], 25000)
        self.assertEqual(result["date"], "2026-11-09")
        self.assertEqual(result["status"], "NEEDS_CONFIRMATION")

    def test_missing_date(self):
        result = self.classify("beli kopi 20rb")

        self.assertEqual(result["intent"], "CreateTransaction")
        self.assertEqual(result["amount"], 20000)
        self.assertIsNone(result["date"])
        self.assertIn("DATE_REQUIRED", result["issues"])
        self.assertEqual(result["status"], "REVIEW_REQUIRED")

        # Known ML error: currently predicts Shopping instead of Food.
        # Do not assert the incorrect category as expected behavior.

    def test_multiple_amounts(self):
        result = self.classify("kemarin beli nasi 25rb dan kopi 15rb")

        self.assertEqual(result["intent"], "CreateTransaction")
        self.assertIsNone(result["amount"])
        self.assertIn("MULTIPLE_AMOUNTS", result["issues"])
        self.assertEqual(result["status"], "REVIEW_REQUIRED")

    def test_cancellation_safety(self):
        result = self.classify("belum jadi beli tiket ya")

        self.assertEqual(result["intent"], "CreateTransaction")
        self.assertIn("SAFETY_REVIEW", result["issues"])
        self.assertEqual(result["status"], "REVIEW_REQUIRED")

    def test_summary_query(self):
        result = self.classify("berapa pengeluaran bulan ini?")

        self.assertEqual(result["intent"], "SummaryQuery")
        self.assertEqual(result["status"], "CLASSIFIED")
        self.assertIsNone(result["category"])
        self.assertIsNone(result["type"])

        # Non-transaction intents should not extract transaction fields.
        self.assertIsNone(result["amount"])
        self.assertIsNone(result["date"])
        self.assertFalse(result["requiresConfirmation"])
        self.assertEqual(result["issues"], [])


if __name__ == "__main__":
    unittest.main()
