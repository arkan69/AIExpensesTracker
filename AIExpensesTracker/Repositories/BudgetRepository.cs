using ExpenseApi.Data;
using ExpenseApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApi.Repositories;

public class BudgetRepository(AppDbContext dbContext) : IBudgetRepository
{
    public async Task<List<Budget>> GetAllAsync(int userId)
    {
        return await CreateReadQuery(userId)
            .OrderByDescending(budget => budget.Year)
            .ThenByDescending(budget => budget.Month)
            .ThenBy(budget => budget.Category!.Name)
            .ToListAsync();
    }

    public async Task<List<Budget>> GetByYearAsync(int userId, int year)
    {
        return await CreateReadQuery(userId)
            .Where(budget => budget.Year == year)
            .OrderByDescending(budget => budget.Month)
            .ThenBy(budget => budget.Category!.Name)
            .ToListAsync();
    }

    public async Task<List<Budget>> GetByYearAndMonthAsync(int userId, int year, int month)
    {
        return await CreateReadQuery(userId)
            .Where(budget => budget.Year == year && budget.Month == month)
            .OrderBy(budget => budget.Category!.Name)
            .ToListAsync();
    }

    public async Task<Budget?> GetByIdAsync(int userId, int id)
    {
        return await CreateReadQuery(userId)
            .FirstOrDefaultAsync(budget => budget.Id == id);
    }

    public async Task<Budget?> GetByIdForUpdateAsync(int userId, int id)
    {
        return await dbContext.Budgets
            .FirstOrDefaultAsync(budget => budget.UserId == userId && budget.Id == id);
    }

    public async Task<bool> ExistsByCategoryMonthYearAsync(int userId, int categoryId, int month, int year, int? excludeId = null)
    {
        return await dbContext.Budgets.AnyAsync(budget =>
            budget.UserId == userId &&
            budget.CategoryId == categoryId &&
            budget.Month == month &&
            budget.Year == year &&
            (!excludeId.HasValue || budget.Id != excludeId.Value));
    }

    public async Task<Budget> AddAsync(Budget budget)
    {
        dbContext.Budgets.Add(budget);
        await dbContext.SaveChangesAsync();
        return budget;
    }

    public async Task UpdateAsync(Budget budget)
    {
        dbContext.Budgets.Update(budget);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Budget budget)
    {
        dbContext.Budgets.Remove(budget);
        await dbContext.SaveChangesAsync();
    }

    private IQueryable<Budget> CreateReadQuery(int userId)
    {
        return dbContext.Budgets
            .AsNoTracking()
            .Include(budget => budget.Category)
            .Where(budget => budget.UserId == userId)
            .AsQueryable();
    }
}
