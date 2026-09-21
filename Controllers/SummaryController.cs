using ExpenseApi.Common.Responses;
using ExpenseApi.DTOs.Summaries;
using ExpenseApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApi.Controllers;

[ApiController]
[Route("api/users/{userId:int}/summary")]
public class SummaryController(ISummaryService summaryService) : ControllerBase
{
    [HttpGet("year/{year:int}/month/{month:int}")]
    public async Task<ActionResult<ApiResponse<MonthlySummaryResponseDto>>> GetMonthlySummary(int userId, int year, int month)
    {
        var result = await summaryService.GetMonthlySummaryAsync(userId, year, month);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<MonthlySummaryResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<MonthlySummaryResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }
}
