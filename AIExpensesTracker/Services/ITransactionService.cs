using ExpenseApi.DTOs.Transactions;

namespace ExpenseApi.Services;

public interface ITransactionService
{
    Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetAllAsync(int userId);

    Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetByYearAsync(int userId, int year);

    Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetByYearAndMonthAsync(int userId, int year, int month);

    Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetByTypeAsync(int userId, string type);

    Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetByYearMonthAndTypeAsync(int userId, int year, int month, string type);

    Task<(bool Success, int StatusCode, string Message, TransactionResponseDto? Data)> GetByIdAsync(int userId, int id);

    Task<(bool Success, int StatusCode, string Message, TransactionResponseDto? Data)> CreateAsync(int userId, CreateTransactionDto dto);

    Task<(bool Success, int StatusCode, string Message, TransactionResponseDto? Data)> UpdateAsync(int userId, int id, UpdateTransactionDto dto);

    Task<(bool Success, int StatusCode, string Message)> DeleteAsync(int userId, int id);
}
