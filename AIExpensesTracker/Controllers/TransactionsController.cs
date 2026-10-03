using ExpenseApi.Common.Responses;
using ExpenseApi.DTOs.Transactions;
using ExpenseApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApi.Controllers;

[ApiController]
[Route("api/users/{userId:int}/transactions")]
public class TransactionsController(ITransactionService transactionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponseDto>>>> GetAll(int userId)
    {
        var result = await transactionService.GetAllAsync(userId);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<TransactionResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<TransactionResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("year/{year:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponseDto>>>> GetByYear(int userId, int year)
    {
        var result = await transactionService.GetByYearAsync(userId, year);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<TransactionResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<TransactionResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("year/{year:int}/month/{month:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponseDto>>>> GetByYearAndMonth(int userId, int year, int month)
    {
        var result = await transactionService.GetByYearAndMonthAsync(userId, year, month);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<TransactionResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<TransactionResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponseDto>>>> GetByType(int userId, string type)
    {
        var result = await transactionService.GetByTypeAsync(userId, type);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<TransactionResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<TransactionResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("year/{year:int}/month/{month:int}/type/{type}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponseDto>>>> GetByYearMonthAndType(int userId, int year, int month, string type)
    {
        var result = await transactionService.GetByYearMonthAndTypeAsync(userId, year, month, type);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<TransactionResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<TransactionResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TransactionResponseDto>>> GetById(int userId, int id)
    {
        var result = await transactionService.GetByIdAsync(userId, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<TransactionResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<TransactionResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionResponseDto>>> Create(int userId, CreateTransactionDto dto)
    {
        var result = await transactionService.CreateAsync(userId, dto);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<TransactionResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return CreatedAtAction(nameof(GetById), new { userId, id = result.Data!.Id }, new ApiResponse<TransactionResponseDto>
        {
            Status = StatusCodes.Status201Created,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<TransactionResponseDto>>> Update(int userId, int id, UpdateTransactionDto dto)
    {
        var result = await transactionService.UpdateAsync(userId, id, dto);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<TransactionResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<TransactionResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int userId, int id)
    {
        var result = await transactionService.DeleteAsync(userId, id);
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
