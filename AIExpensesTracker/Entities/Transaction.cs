namespace ExpenseApi.Entities;

public class Transaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string TransactionType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int CategoryId { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }

    public Category? Category { get; set; }
}
