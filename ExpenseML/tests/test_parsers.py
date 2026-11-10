
import unittest
from datetime import date

from src.amount_parser import parse_amount
from src.date_parser import parse_date
from src.transaction_parser import parse_transaction


class TestAmountParser(unittest.TestCase):
    def test_amount_scenarios(self):
        cases = [
            ("beli kopi 20000 tanggal 5 November 2026", "OK", 20000),
            ("5 November 2026 beli kopi 20rb", "OK", 20000),
            ("beli kopi 20rb tahun 2026", "OK", 20000),
            ("transfer 50000 ke rekening 1234567890", "OK", 50000),
            ("bayar listrik Rp150000 kemarin", "OK", 150000),
            ("beli makan 15000", "OK", 15000),
            ("beli nasi 25rb", "OK", 25000),
            ("bayar kos 1,5jt", "OK", 1500000),
            ("bayar listrik 150.000", "OK", 150000),
            ("beli 2 porsi nasi 25000", "OK", 25000),
            ("beli nasi 25000 dan kopi 15000", "MULTIPLE_AMOUNTS", None),
            ("beli makanan", "MISSING_AMOUNT", None),
        ]

        for message, expected_status, expected_amount in cases:
            with self.subTest(message=message):
                result = parse_amount(message)
                self.assertEqual(result["status"], expected_status)
                self.assertEqual(result["amount"], expected_amount)


class TestDateParser(unittest.TestCase):
    def test_date_scenarios(self):
        reference = date(2026, 11, 10)

        cases = [
            ("beli nasi 25rb", "DATE_REQUIRED", None),
            ("hari ini beli nasi 25rb", "OK", "2026-11-10"),
            ("kemarin beli nasi 25rb", "OK", "2026-11-09"),
            ("kemarin lusa beli kopi 20rb", "OK", "2026-11-08"),
            ("tanggal 5 November beli kopi 20rb", "OK", "2026-11-05"),
            ("5 November 2026 beli kopi 20rb", "OK", "2026-11-05"),
            ("beli kopi 20rb tanggal 5", "DATE_AMBIGUOUS", None),
            ("tanggal 32 November beli kopi 20rb", "INVALID_DATE", None),
        ]

        for message, expected_status, expected_date in cases:
            with self.subTest(message=message):
                result = parse_date(message, reference)
                self.assertEqual(result["status"], expected_status)
                self.assertEqual(result["date"], expected_date)


class TestTransactionParser(unittest.TestCase):
    def test_transaction_scenarios(self):
        reference = date(2026, 11, 10)

        cases = [
            ("kemarin beli nasi padang 25rb", "READY_FOR_CONFIRMATION"),
            ("hari ini bayar listrik 150.000", "READY_FOR_CONFIRMATION"),
            ("tanggal 5 November bayar kos 1,5jt", "READY_FOR_CONFIRMATION"),
            ("beli kopi 20rb", "REVIEW_REQUIRED"),
            ("kemarin beli makanan", "REVIEW_REQUIRED"),
            ("kemarin beli nasi 25rb dan kopi 15rb", "REVIEW_REQUIRED"),
            ("tanggal 32 November beli kopi 20rb", "REVIEW_REQUIRED"),
            ("beli 2 porsi nasi 25rb kemarin", "READY_FOR_CONFIRMATION"),
        ]

        for message, expected_status in cases:
            with self.subTest(message=message):
                result = parse_transaction(message, reference)
                self.assertEqual(result["status"], expected_status)
                self.assertTrue(result["requiresConfirmation"])


if __name__ == "__main__":
    unittest.main()
