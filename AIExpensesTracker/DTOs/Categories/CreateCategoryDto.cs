using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.DTOs.Categories;

public class CreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Income|Expense)$", ErrorMessage = "Type must be either Income or Expense.")]
    public string Type { get; set; } = string.Empty;
}
