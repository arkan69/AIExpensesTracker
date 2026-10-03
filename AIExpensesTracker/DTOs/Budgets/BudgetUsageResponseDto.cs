namespace ExpenseApi.DTOs.Budgets;

public class BudgetUsageResponseDto
{
    public int BudgetId { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal LimitAmount { get; set; }

    public decimal SpentAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public decimal UsagePercentage { get; set; }

    public string Status { get; set; } = string.Empty;
}
