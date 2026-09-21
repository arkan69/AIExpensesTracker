using ExpenseApi.Entities;

namespace ExpenseApi.Repositories;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetAllAsync(int userId);

    Task<List<Transaction>> GetByYearAsync(int userId, int year);

    Task<List<Transaction>> GetByYearAndMonthAsync(int userId, int year, int month);

    Task<List<Transaction>> GetByTypeAsync(int userId, string type);

    Task<List<Transaction>> GetByYearMonthAndTypeAsync(int userId, int year, int month, string type);

    Task<Transaction?> GetByIdAsync(int userId, int id);

    Task<Transaction?> GetByIdForUpdateAsync(int userId, int id);

    Task<Transaction> AddAsync(Transaction transaction);

    Task UpdateAsync(Transaction transaction);

    Task DeleteAsync(Transaction transaction);
}
