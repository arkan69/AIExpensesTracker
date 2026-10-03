using ExpenseApi.DTOs.Budgets;
using ExpenseApi.Entities;
using ExpenseApi.Repositories;
using Microsoft.AspNetCore.Http;

namespace ExpenseApi.Services;

public class BudgetService(
    IBudgetRepository budgetRepository,
    ICategoryRepository categoryRepository,
    IUserRepository userRepository,
    ITransactionRepository transactionRepository) : IBudgetService
{
    public async Task<(bool Success, int StatusCode, string Message, List<BudgetResponseDto>? Data)> GetAllAsync(int userId)
    {
        var user = await userRepository.GetActiveByIdAsync(userId);
        if (user is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        var budgets = await budgetRepository.GetAllAsync(userId);
        return BuildCollectionResponse(budgets, "Success");
    }

    public async Task<(bool Success, int StatusCode, string Message, List<BudgetResponseDto>? Data)> GetByYearAsync(int userId, int year)
    {
        var user = await userRepository.GetActiveByIdAsync(userId);
        if (user is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        if (year <= 0)
        {
            return (false, StatusCodes.Status400BadRequest, "Year must be greater than 0.", null);
        }

        var budgets = await budgetRepository.GetByYearAsync(userId, year);
        return BuildCollectionResponse(budgets, "Success");
    }

    public async Task<(bool Success, int StatusCode, string Message, List<BudgetResponseDto>? Data)> GetByYearAndMonthAsync(int userId, int year, int month)
    {
        var user = await userRepository.GetActiveByIdAsync(userId);
        if (user is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        if (year <= 0)
        {
            return (false, StatusCodes.Status400BadRequest, "Year must be greater than 0.", null);
        }

        if (month < 1 || month > 12)
        {
            return (false, StatusCodes.Status400BadRequest, "Month must be between 1 and 12.", null);
        }

        var budgets = await budgetRepository.GetByYearAndMonthAsync(userId, year, month);
        return BuildCollectionResponse(budgets, "Success");
    }

    public async Task<(bool Success, int StatusCode, string Message, List<BudgetUsageResponseDto>? Data)> GetUsageByYearAndMonthAsync(int userId, int year, int month)
    {
        var user = await userRepository.GetActiveByIdAsync(userId);
        if (user is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        if (year <= 0)
        {
            return (false, StatusCodes.Status400BadRequest, "Year must be greater than 0.", null);
        }

        if (month < 1 || month > 12)
        {
            return (false, StatusCodes.Status400BadRequest, "Month must be between 1 and 12.", null);
        }

        var budgets = await budgetRepository.GetByYearAndMonthAsync(userId, year, month);
        if (budgets.Count == 0)
        {
            return (false, StatusCodes.Status404NotFound, "No budgets found.", null);
        }

        var transactions = await transactionRepository.GetByYearAndMonthAsync(userId, year, month);

        var expenseTotalsByCategory = transactions
            .Where(transaction => string.Equals(transaction.TransactionType, "Expense", StringComparison.OrdinalIgnoreCase))
            .GroupBy(transaction => transaction.CategoryId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(transaction => transaction.Amount));

        var usage = budgets
            .Select(budget =>
            {
                var spentAmount = expenseTotalsByCategory.GetValueOrDefault(budget.CategoryId, 0m);
                var remainingAmount = budget.LimitAmount - spentAmount;
                var usagePercentage = budget.LimitAmount == 0
                    ? 0
                    : (spentAmount / budget.LimitAmount) * 100;

                return new BudgetUsageResponseDto
                {
                    BudgetId = budget.Id,
                    CategoryId = budget.CategoryId,
                    CategoryName = budget.Category?.Name ?? string.Empty,
                    LimitAmount = budget.LimitAmount,
                    SpentAmount = spentAmount,
                    RemainingAmount = remainingAmount,
                    UsagePercentage = usagePercentage,
                    Status = GetBudgetStatus(usagePercentage)
                };
            })
            .OrderByDescending(item => item.UsagePercentage)
            .ThenBy(item => item.CategoryName)
            .ToList();

        return (true, StatusCodes.Status200OK, "Success", usage);
    }

    public async Task<(bool Success, int StatusCode, string Message, BudgetResponseDto? Data)> GetByIdAsync(int userId, int id)
    {
        var user = await userRepository.GetActiveByIdAsync(userId);
        if (user is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        var budget = await budgetRepository.GetByIdAsync(userId, id);
        if (budget is null)
        {
            return (false, StatusCodes.Status404NotFound, "Budget not found.", null);
        }

        return (true, StatusCodes.Status200OK, "Success", MapToResponse(budget));
    }

    public async Task<(bool Success, int StatusCode, string Message, BudgetResponseDto? Data)> CreateAsync(int userId, CreateBudgetDto dto)
    {
        var validationResult = await ValidateBudgetAsync(userId, dto.CategoryId, dto.Month, dto.Year, dto.LimitAmount);
        if (!validationResult.Success)
        {
            return (false, validationResult.StatusCode, validationResult.Message, null);
        }

        var category = validationResult.Category!;

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Month = dto.Month,
            Year = dto.Year,
            LimitAmount = dto.LimitAmount,
            CreatedAt = DateTime.UtcNow
        };

        var createdBudget = await budgetRepository.AddAsync(budget);
        createdBudget.Category = category;

        return (true, StatusCodes.Status201Created, "Budget created successfully.", MapToResponse(createdBudget));
    }

    public async Task<(bool Success, int StatusCode, string Message, BudgetResponseDto? Data)> UpdateAsync(int userId, int id, UpdateBudgetDto dto)
    {
        var user = await userRepository.GetActiveByIdAsync(userId);
        if (user is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        var existingBudget = await budgetRepository.GetByIdForUpdateAsync(userId, id);
        if (existingBudget is null)
        {
            return (false, StatusCodes.Status404NotFound, "Budget not found.", null);
        }

        var validationResult = await ValidateBudgetAsync(userId, dto.CategoryId, dto.Month, dto.Year, dto.LimitAmount, id);
        if (!validationResult.Success)
        {
            return (false, validationResult.StatusCode, validationResult.Message, null);
        }

        var category = validationResult.Category!;

        existingBudget.CategoryId = dto.CategoryId;
        existingBudget.Month = dto.Month;
        existingBudget.Year = dto.Year;
        existingBudget.LimitAmount = dto.LimitAmount;

        await budgetRepository.UpdateAsync(existingBudget);
        existingBudget.Category = category;

        return (true, StatusCodes.Status200OK, "Budget updated successfully.", MapToResponse(existingBudget));
    }

    public async Task<(bool Success, int StatusCode, string Message)> DeleteAsync(int userId, int id)
    {
        var user = await userRepository.GetActiveByIdAsync(userId);
        if (user is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.");
        }

        var existingBudget = await budgetRepository.GetByIdForUpdateAsync(userId, id);
        if (existingBudget is null)
        {
            return (false, StatusCodes.Status404NotFound, "Budget not found.");
        }

        await budgetRepository.DeleteAsync(existingBudget);
        return (true, StatusCodes.Status200OK, "Budget deleted successfully.");
    }

    private async Task<(bool Success, int StatusCode, string Message, Category? Category)> ValidateBudgetAsync(
        int userId,
        int categoryId,
        int month,
        int year,
        decimal limitAmount,
        int? excludeId = null)
    {
        var user = await userRepository.GetActiveByIdAsync(userId);
        if (user is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        if (month < 1 || month > 12)
        {
            return (false, StatusCodes.Status400BadRequest, "Month must be between 1 and 12.", null);
        }

        if (year <= 0)
        {
            return (false, StatusCodes.Status400BadRequest, "Year must be greater than 0.", null);
        }

        if (limitAmount <= 0)
        {
            return (false, StatusCodes.Status400BadRequest, "LimitAmount must be greater than 0.", null);
        }

        var category = await categoryRepository.GetActiveByIdAsync(categoryId);
        if (category is null)
        {
            return (false, StatusCodes.Status404NotFound, "Category not found or inactive.", null);
        }

        if (!string.Equals(category.Type, "Expense", StringComparison.OrdinalIgnoreCase))
        {
            return (false, StatusCodes.Status400BadRequest, "Budget category must be an Expense category.", null);
        }

        var alreadyExists = await budgetRepository.ExistsByCategoryMonthYearAsync(userId, categoryId, month, year, excludeId);
        if (alreadyExists)
        {
            return (false, StatusCodes.Status409Conflict, "A budget for the same category, month, and year already exists.", null);
        }

        return (true, StatusCodes.Status200OK, string.Empty, category);
    }

    private static (bool Success, int StatusCode, string Message, List<BudgetResponseDto>? Data) BuildCollectionResponse(
        List<Budget> budgets,
        string successMessage)
    {
        if (budgets.Count == 0)
        {
            return (false, StatusCodes.Status404NotFound, "No budgets found.", null);
        }

        return (true, StatusCodes.Status200OK, successMessage, budgets.Select(MapToResponse).ToList());
    }

    private static BudgetResponseDto MapToResponse(Budget budget)
    {
        return new BudgetResponseDto
        {
            Id = budget.Id,
            UserId = budget.UserId,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category?.Name ?? string.Empty,
            Month = budget.Month,
            Year = budget.Year,
            LimitAmount = budget.LimitAmount,
            CreatedAt = budget.CreatedAt
        };
    }

    private static string GetBudgetStatus(decimal usagePercentage)
    {
        if (usagePercentage >= 100)
        {
            return "Exceeded";
        }

        if (usagePercentage >= 80)
        {
            return "Warning";
        }

        return "Safe";
    }
}
