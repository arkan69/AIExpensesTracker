namespace ExpenseApi.DTOs.Summaries;

public class ExpenseByCategoryDto
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
