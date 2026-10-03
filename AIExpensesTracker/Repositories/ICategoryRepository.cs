using ExpenseApi.Entities;

namespace ExpenseApi.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetActiveAsync();

    Task<Category?> GetActiveByIdAsync(int id);

    Task<bool> ExistsByNameAndTypeAsync(string name, string type, int? excludeId = null);

    Task<Category> AddAsync(Category category);

    Task UpdateAsync(Category category);
}
