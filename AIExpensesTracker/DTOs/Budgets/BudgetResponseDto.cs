namespace ExpenseApi.DTOs.Budgets;

public class BudgetResponseDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal LimitAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}
