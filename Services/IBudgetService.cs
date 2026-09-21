using ExpenseApi.DTOs.Budgets;

namespace ExpenseApi.Services;

public interface IBudgetService
{
    Task<(bool Success, int StatusCode, string Message, List<BudgetResponseDto>? Data)> GetAllAsync(int userId);

    Task<(bool Success, int StatusCode, string Message, List<BudgetResponseDto>? Data)> GetByYearAsync(int userId, int year);

    Task<(bool Success, int StatusCode, string Message, List<BudgetResponseDto>? Data)> GetByYearAndMonthAsync(int userId, int year, int month);

    Task<(bool Success, int StatusCode, string Message, List<BudgetUsageResponseDto>? Data)> GetUsageByYearAndMonthAsync(int userId, int year, int month);

    Task<(bool Success, int StatusCode, string Message, BudgetResponseDto? Data)> GetByIdAsync(int userId, int id);

    Task<(bool Success, int StatusCode, string Message, BudgetResponseDto? Data)> CreateAsync(int userId, CreateBudgetDto dto);

    Task<(bool Success, int StatusCode, string Message, BudgetResponseDto? Data)> UpdateAsync(int userId, int id, UpdateBudgetDto dto);

    Task<(bool Success, int StatusCode, string Message)> DeleteAsync(int userId, int id);
}
