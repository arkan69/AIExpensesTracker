using ExpenseApi.DTOs.Categories;

namespace ExpenseApi.Services;

public interface ICategoryService
{
    Task<(bool Success, int StatusCode, string Message, List<CategoryResponseDto>? Data)> GetAllAsync();

    Task<(bool Success, int StatusCode, string Message, CategoryResponseDto? Data)> GetByIdAsync(int id);

    Task<(bool Success, int StatusCode, string Message, CategoryResponseDto? Data)> CreateAsync(CreateCategoryDto dto);

    Task<(bool Success, int StatusCode, string Message, CategoryResponseDto? Data)> UpdateAsync(int id, UpdateCategoryDto dto);

    Task<(bool Success, int StatusCode, string Message)> DeleteAsync(int id);
}
