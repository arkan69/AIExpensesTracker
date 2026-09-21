using ExpenseApi.Data;
using ExpenseApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApi.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<List<User>> GetActiveAsync()
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsActive)
            .OrderBy(user => user.Name)
            .ToListAsync();
    }

    public async Task<User?> GetActiveByIdAsync(int id)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == id && user.IsActive);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await dbContext.Users.AnyAsync(user =>
            user.IsActive &&
            user.Email.ToLower() == normalizedEmail &&
            (!excludeId.HasValue || user.Id != excludeId.Value));
    }

    public async Task<User> AddAsync(User user)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();
    }
}
