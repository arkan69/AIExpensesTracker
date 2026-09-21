using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.DTOs.Transactions;

public class CreateTransactionDto
{
    [Required]
    [RegularExpression("^(Income|Expense)$", ErrorMessage = "TransactionType must be either Income or Expense.")]
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0.")]
    public int CategoryId { get; set; }

    [Required]
    public DateTime? TransactionDate { get; set; }

    public string? Description { get; set; }
}
