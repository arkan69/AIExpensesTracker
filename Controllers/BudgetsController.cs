using ExpenseApi.Common.Responses;
using ExpenseApi.DTOs.Budgets;
using ExpenseApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApi.Controllers;

[ApiController]
[Route("api/users/{userId:int}/budgets")]
public class BudgetsController(IBudgetService budgetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<BudgetResponseDto>>>> GetAll(int userId)
    {
        var result = await budgetService.GetAllAsync(userId);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<BudgetResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<BudgetResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("year/{year:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BudgetResponseDto>>>> GetByYear(int userId, int year)
    {
        var result = await budgetService.GetByYearAsync(userId, year);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<BudgetResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<BudgetResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("year/{year:int}/month/{month:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BudgetResponseDto>>>> GetByYearAndMonth(int userId, int year, int month)
    {
        var result = await budgetService.GetByYearAndMonthAsync(userId, year, month);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<BudgetResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<BudgetResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("year/{year:int}/month/{month:int}/usage")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BudgetUsageResponseDto>>>> GetUsageByYearAndMonth(int userId, int year, int month)
    {
        var result = await budgetService.GetUsageByYearAndMonthAsync(userId, year, month);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<BudgetUsageResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<BudgetUsageResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BudgetResponseDto>>> GetById(int userId, int id)
    {
        var result = await budgetService.GetByIdAsync(userId, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<BudgetResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<BudgetResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BudgetResponseDto>>> Create(int userId, CreateBudgetDto dto)
    {
        var result = await budgetService.CreateAsync(userId, dto);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<BudgetResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return CreatedAtAction(nameof(GetById), new { userId, id = result.Data!.Id }, new ApiResponse<BudgetResponseDto>
        {
            Status = StatusCodes.Status201Created,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<BudgetResponseDto>>> Update(int userId, int id, UpdateBudgetDto dto)
    {
        var result = await budgetService.UpdateAsync(userId, id, dto);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<BudgetResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<BudgetResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int userId, int id)
    {
        var result = await budgetService.DeleteAsync(userId, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<object>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<object>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = null
        });
    }
}
