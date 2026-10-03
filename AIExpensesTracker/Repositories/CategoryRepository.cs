using ExpenseApi.Data;
using ExpenseApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApi.Repositories;

public class CategoryRepository(AppDbContext dbContext) : ICategoryRepository
{
    public async Task<List<Category>> GetActiveAsync()
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetActiveByIdAsync(int id)
    {
        return await dbContext.Categories
            .FirstOrDefaultAsync(category => category.Id == id && category.IsActive);
    }

    public async Task<bool> ExistsByNameAndTypeAsync(string name, string type, int? excludeId = null)
    {
        var normalizedName = name.Trim().ToLower();
        var normalizedType = type.Trim().ToLower();

        return await dbContext.Categories.AnyAsync(category =>
            category.IsActive &&
            category.Name.ToLower() == normalizedName &&
            category.Type.ToLower() == normalizedType &&
            (!excludeId.HasValue || category.Id != excludeId.Value));
    }

    public async Task<Category> AddAsync(Category category)
    {
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        dbContext.Categories.Update(category);
        await dbContext.SaveChangesAsync();
    }
}
