using ExpenseApi.Entities;

namespace ExpenseApi.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetActiveAsync();

    Task<User?> GetActiveByIdAsync(int id);

    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);

    Task<User> AddAsync(User user);

    Task UpdateAsync(User user);
}
