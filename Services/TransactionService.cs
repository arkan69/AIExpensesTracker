using ExpenseApi.DTOs.Transactions;
using ExpenseApi.Entities;
using ExpenseApi.Repositories;
using Microsoft.AspNetCore.Http;

namespace ExpenseApi.Services;

public class TransactionService(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    IUserRepository userRepository) : ITransactionService
{
    public async Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetAllAsync(int userId)
    {
        var userExists = await userRepository.GetActiveByIdAsync(userId);
        if (userExists is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        var transactions = await transactionRepository.GetAllAsync(userId);
        return BuildCollectionResponse(transactions, "Success");
    }

    public async Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetByYearAsync(int userId, int year)
    {
        var userExists = await userRepository.GetActiveByIdAsync(userId);
        if (userExists is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        if (year <= 0)
        {
            return (false, StatusCodes.Status400BadRequest, "Year must be greater than 0.", null);
        }

        var transactions = await transactionRepository.GetByYearAsync(userId, year);
        return BuildCollectionResponse(transactions, "Success");
    }

    public async Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetByYearAndMonthAsync(int userId, int year, int month)
    {
        var userExists = await userRepository.GetActiveByIdAsync(userId);
        if (userExists is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        if (year <= 0)
        {
            return (false, StatusCodes.Status400BadRequest, "Year must be greater than 0.", null);
        }

        if (month < 1 || month > 12)
        {
            return (false, StatusCodes.Status400BadRequest, "Month must be between 1 and 12.", null);
        }

        var transactions = await transactionRepository.GetByYearAndMonthAsync(userId, year, month);
        return BuildCollectionResponse(transactions, "Success");
    }

    public async Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetByTypeAsync(int userId, string type)
    {
        var userExists = await userRepository.GetActiveByIdAsync(userId);
        if (userExists is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        if (!IsValidTransactionType(type))
        {
            return (false, StatusCodes.Status400BadRequest, "Type must be either Income or Expense.", null);
        }

        var transactions = await transactionRepository.GetByTypeAsync(userId, type);
        return BuildCollectionResponse(transactions, "Success");
    }

    public async Task<(bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data)> GetByYearMonthAndTypeAsync(int userId, int year, int month, string type)
    {
        var userExists = await userRepository.GetActiveByIdAsync(userId);
        if (userExists is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        if (year <= 0)
        {
            return (false, StatusCodes.Status400BadRequest, "Year must be greater than 0.", null);
        }

        if (month < 1 || month > 12)
        {
            return (false, StatusCodes.Status400BadRequest, "Month must be between 1 and 12.", null);
        }

        if (!IsValidTransactionType(type))
        {
            return (false, StatusCodes.Status400BadRequest, "Type must be either Income or Expense.", null);
        }

        var transactions = await transactionRepository.GetByYearMonthAndTypeAsync(userId, year, month, type);
        return BuildCollectionResponse(transactions, "Success");
    }

    public async Task<(bool Success, int StatusCode, string Message, TransactionResponseDto? Data)> GetByIdAsync(int userId, int id)
    {
        var userExists = await userRepository.GetActiveByIdAsync(userId);
        if (userExists is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        var transaction = await transactionRepository.GetByIdAsync(userId, id);
        if (transaction is null)
        {
            return (false, StatusCodes.Status404NotFound, "Transaction not found.", null);
        }

        return (true, StatusCodes.Status200OK, "Success", MapToResponse(transaction));
    }

    public async Task<(bool Success, int StatusCode, string Message, TransactionResponseDto? Data)> CreateAsync(int userId, CreateTransactionDto dto)
    {
        var validationResult = await ValidateTransactionAsync(userId, dto.TransactionType, dto.Amount, dto.CategoryId, dto.TransactionDate);
        if (!validationResult.Success)
        {
            return (false, validationResult.StatusCode, validationResult.Message, null);
        }

        var category = validationResult.Category!;

        var transaction = new Transaction
        {
            UserId = userId,
            TransactionType = dto.TransactionType.Trim(),
            Amount = dto.Amount,
            CategoryId = dto.CategoryId,
            TransactionDate = dto.TransactionDate!.Value,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        var createdTransaction = await transactionRepository.AddAsync(transaction);
        createdTransaction.Category = category;

        return (true, StatusCodes.Status201Created, "Transaction created successfully.", MapToResponse(createdTransaction));
    }

    public async Task<(bool Success, int StatusCode, string Message, TransactionResponseDto? Data)> UpdateAsync(int userId, int id, UpdateTransactionDto dto)
    {
        var userExists = await userRepository.GetActiveByIdAsync(userId);
        if (userExists is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        var existingTransaction = await transactionRepository.GetByIdForUpdateAsync(userId, id);
        if (existingTransaction is null)
        {
            return (false, StatusCodes.Status404NotFound, "Transaction not found.", null);
        }

        var validationResult = await ValidateTransactionAsync(userId, dto.TransactionType, dto.Amount, dto.CategoryId, dto.TransactionDate);
        if (!validationResult.Success)
        {
            return (false, validationResult.StatusCode, validationResult.Message, null);
        }

        var category = validationResult.Category!;

        existingTransaction.TransactionType = dto.TransactionType.Trim();
        existingTransaction.Amount = dto.Amount;
        existingTransaction.CategoryId = dto.CategoryId;
        existingTransaction.TransactionDate = dto.TransactionDate!.Value;
        existingTransaction.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        existingTransaction.UpdatedAt = DateTime.UtcNow;

        await transactionRepository.UpdateAsync(existingTransaction);
        existingTransaction.Category = category;

        return (true, StatusCodes.Status200OK, "Transaction updated successfully.", MapToResponse(existingTransaction));
    }

    public async Task<(bool Success, int StatusCode, string Message)> DeleteAsync(int userId, int id)
    {
        var userExists = await userRepository.GetActiveByIdAsync(userId);
        if (userExists is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.");
        }

        var existingTransaction = await transactionRepository.GetByIdForUpdateAsync(userId, id);
        if (existingTransaction is null)
        {
            return (false, StatusCodes.Status404NotFound, "Transaction not found.");
        }

        await transactionRepository.DeleteAsync(existingTransaction);
        return (true, StatusCodes.Status200OK, "Transaction deleted successfully.");
    }

    private async Task<(bool Success, int StatusCode, string Message, TransactionResponseDto? Data, Category? Category)> ValidateTransactionAsync(
        int userId,
        string transactionType,
        decimal amount,
        int categoryId,
        DateTime? transactionDate)
    {
        var userExists = await userRepository.GetActiveByIdAsync(userId);
        if (userExists is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null, null);
        }

        if (!IsValidTransactionType(transactionType))
        {
            return (false, StatusCodes.Status400BadRequest, "TransactionType must be either Income or Expense.", null, null);
        }

        if (amount <= 0)
        {
            return (false, StatusCodes.Status400BadRequest, "Amount must be greater than 0.", null, null);
        }

        if (!transactionDate.HasValue)
        {
            return (false, StatusCodes.Status400BadRequest, "TransactionDate is required.", null, null);
        }

        var category = await categoryRepository.GetActiveByIdAsync(categoryId);
        if (category is null)
        {
            return (false, StatusCodes.Status404NotFound, "Category not found.", null, null);
        }

        if (!string.Equals(transactionType.Trim(), category.Type, StringComparison.OrdinalIgnoreCase))
        {
            return (false, StatusCodes.Status400BadRequest, "TransactionType must match the selected category type.", null, null);
        }

        return (true, StatusCodes.Status200OK, string.Empty, null, category);
    }

    private static bool IsValidTransactionType(string? transactionType)
    {
        return string.Equals(transactionType?.Trim(), "Income", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(transactionType?.Trim(), "Expense", StringComparison.OrdinalIgnoreCase);
    }

    private static (bool Success, int StatusCode, string Message, List<TransactionResponseDto>? Data) BuildCollectionResponse(
        List<Transaction> transactions,
        string successMessage)
    {
        if (transactions.Count == 0)
        {
            return (false, StatusCodes.Status404NotFound, "No transactions found.", null);
        }

        return (true, StatusCodes.Status200OK, successMessage, transactions.Select(MapToResponse).ToList());
    }

    private static TransactionResponseDto MapToResponse(Transaction transaction)
    {
        return new TransactionResponseDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            TransactionType = transaction.TransactionType,
            Amount = transaction.Amount,
            CategoryId = transaction.CategoryId,
            CategoryName = transaction.Category?.Name ?? string.Empty,
            TransactionDate = transaction.TransactionDate,
            Description = transaction.Description,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }
}
