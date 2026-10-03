using ExpenseApi.Common.Responses;
using ExpenseApi.DTOs.Categories;
using ExpenseApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponseDto>>>> GetAll()
    {
        var result = await categoryService.GetAllAsync();
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<IEnumerable<CategoryResponseDto>>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<CategoryResponseDto>>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetById(int id)
    {
        var result = await categoryService.GetByIdAsync(id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<CategoryResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<CategoryResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Create(CreateCategoryDto dto)
    {
        var result = await categoryService.CreateAsync(dto);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<CategoryResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, new ApiResponse<CategoryResponseDto>
        {
            Status = StatusCodes.Status201Created,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Update(int id, UpdateCategoryDto dto)
    {
        var result = await categoryService.UpdateAsync(id, dto);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<CategoryResponseDto>
            {
                Status = result.StatusCode,
                Message = result.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<CategoryResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Message = result.Message,
            Data = result.Data
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await categoryService.DeleteAsync(id);
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
