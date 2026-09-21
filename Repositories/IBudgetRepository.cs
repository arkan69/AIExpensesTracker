using ExpenseApi.Entities;

namespace ExpenseApi.Repositories;

public interface IBudgetRepository
{
    Task<List<Budget>> GetAllAsync(int userId);

    Task<List<Budget>> GetByYearAsync(int userId, int year);

    Task<List<Budget>> GetByYearAndMonthAsync(int userId, int year, int month);

    Task<Budget?> GetByIdAsync(int userId, int id);

    Task<Budget?> GetByIdForUpdateAsync(int userId, int id);

    Task<bool> ExistsByCategoryMonthYearAsync(int userId, int categoryId, int month, int year, int? excludeId = null);

    Task<Budget> AddAsync(Budget budget);

    Task UpdateAsync(Budget budget);

    Task DeleteAsync(Budget budget);
}
