namespace ExpenseApi.Entities;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
