using ExpenseApi.Data;
using ExpenseApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApi.Repositories;

public class TransactionRepository(AppDbContext dbContext) : ITransactionRepository
{
    public async Task<List<Transaction>> GetAllAsync(int userId)
    {
        return await CreateReadQuery(userId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByYearAsync(int userId, int year)
    {
        return await CreateReadQuery(userId)
            .Where(transaction => transaction.TransactionDate.Year == year)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByYearAndMonthAsync(int userId, int year, int month)
    {
        return await CreateReadQuery(userId)
            .Where(transaction => transaction.TransactionDate.Year == year &&
                                  transaction.TransactionDate.Month == month)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByTypeAsync(int userId, string type)
    {
        var normalizedType = type.Trim().ToLower();

        return await CreateReadQuery(userId)
            .Where(transaction => transaction.TransactionType.ToLower() == normalizedType)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByYearMonthAndTypeAsync(int userId, int year, int month, string type)
    {
        var normalizedType = type.Trim().ToLower();

        return await CreateReadQuery(userId)
            .Where(transaction => transaction.TransactionDate.Year == year &&
                                  transaction.TransactionDate.Month == month &&
                                  transaction.TransactionType.ToLower() == normalizedType)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int userId, int id)
    {
        return await CreateReadQuery(userId)
            .FirstOrDefaultAsync(transaction => transaction.Id == id);
    }

    public async Task<Transaction?> GetByIdForUpdateAsync(int userId, int id)
    {
        return await dbContext.Transactions
            .FirstOrDefaultAsync(transaction => transaction.UserId == userId && transaction.Id == id);
    }

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        dbContext.Transactions.Update(transaction);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        dbContext.Transactions.Remove(transaction);
        await dbContext.SaveChangesAsync();
    }

    private IQueryable<Transaction> CreateReadQuery(int userId)
    {
        return dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Where(transaction => transaction.UserId == userId)
            .AsQueryable();
    }
}
