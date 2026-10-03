using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.DTOs.Budgets;

public class UpdateBudgetDto
{
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0.")]
    public int CategoryId { get; set; }

    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
    public int Month { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Year must be greater than 0.")]
    public int Year { get; set; }
    public decimal LimitAmount { get; set; }
}
