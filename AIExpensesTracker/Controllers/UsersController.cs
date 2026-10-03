using ExpenseApi.Common.Responses;
using ExpenseApi.DTOs.Users;
using ExpenseApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetAll()
    {
        var result = await userService.GetAllAsync();
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<UserResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<UserResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetById(int id)
    {
        var user = await userService.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound(new ApiResponse<UserResponseDto>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "User not found.",
                Data = null
            });
        }

        return Ok(new ApiResponse<UserResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Message = "Success",
            Data = user
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Create(CreateUserDto dto)
    {
        var result = await userService.CreateAsync(dto);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<UserResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, new ApiResponse<UserResponseDto>
        {
            Status = StatusCodes.Status201Created,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Update(int id, UpdateUserDto dto)
    {
        var result = await userService.UpdateAsync(id, dto);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<UserResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<UserResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await userService.DeleteAsync(id);
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
