using ExpenseApi.DTOs.Summaries;

namespace ExpenseApi.Services;

public interface ISummaryService
{
    Task<(bool Success, int StatusCode, string Message, MonthlySummaryResponseDto? Data)> GetMonthlySummaryAsync(int userId, int year, int month);
}
