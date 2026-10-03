namespace ExpenseApi.Entities;

public class Budget
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal LimitAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }

    public Category? Category { get; set; }
}
