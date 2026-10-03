using ExpenseApi.DTOs.Summaries;
using ExpenseApi.Repositories;
using Microsoft.AspNetCore.Http;

namespace ExpenseApi.Services;

public class SummaryService(
    IUserRepository userRepository,
    ITransactionRepository transactionRepository) : ISummaryService
{
    public async Task<(bool Success, int StatusCode, string Message, MonthlySummaryResponseDto? Data)> GetMonthlySummaryAsync(int userId, int year, int month)
    {
        var user = await userRepository.GetActiveByIdAsync(userId);
        if (user is null)
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
        if (transactions.Count == 0)
        {
            return (false, StatusCodes.Status404NotFound, "No transactions found for the requested period.", null);
        }

        var totalIncome = transactions
            .Where(transaction => string.Equals(transaction.TransactionType, "Income", StringComparison.OrdinalIgnoreCase))
            .Sum(transaction => transaction.Amount);

        var totalExpense = transactions
            .Where(transaction => string.Equals(transaction.TransactionType, "Expense", StringComparison.OrdinalIgnoreCase))
            .Sum(transaction => transaction.Amount);

        var expenseByCategory = transactions
            .Where(transaction => string.Equals(transaction.TransactionType, "Expense", StringComparison.OrdinalIgnoreCase))
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                CategoryName = transaction.Category?.Name ?? string.Empty
            })
            .Select(group => new ExpenseByCategoryDto
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.CategoryName,
                Amount = group.Sum(transaction => transaction.Amount)
            })
            .OrderByDescending(item => item.Amount)
            .ToList();

        var summary = new MonthlySummaryResponseDto
        {
            UserId = userId,
            Year = year,
            Month = month,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = totalIncome - totalExpense,
            ExpenseByCategory = expenseByCategory
        };

        return (true, StatusCodes.Status200OK, "Success", summary);
    }
}
