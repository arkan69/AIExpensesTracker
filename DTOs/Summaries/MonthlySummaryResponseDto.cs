namespace ExpenseApi.DTOs.Summaries;

public class MonthlySummaryResponseDto
{
    public int UserId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal Balance { get; set; }

    public List<ExpenseByCategoryDto> ExpenseByCategory { get; set; } = [];
}
