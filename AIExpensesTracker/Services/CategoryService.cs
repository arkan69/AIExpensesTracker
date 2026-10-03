using ExpenseApi.DTOs.Categories;
using ExpenseApi.Entities;
using ExpenseApi.Repositories;
using Microsoft.AspNetCore.Http;

namespace ExpenseApi.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<(bool Success, int StatusCode, string Message, List<CategoryResponseDto>? Data)> GetAllAsync()
    {
        var categories = await categoryRepository.GetActiveAsync();
        if (categories.Count == 0)
        {
            return (false, StatusCodes.Status404NotFound, "No categories found.", null);
        }

        return (true, StatusCodes.Status200OK, "Success", categories.Select(MapToResponse).ToList());
    }

    public async Task<(bool Success, int StatusCode, string Message, CategoryResponseDto? Data)> GetByIdAsync(int id)
    {
        var category = await categoryRepository.GetActiveByIdAsync(id);
        if (category is null)
        {
            return (false, StatusCodes.Status404NotFound, "Category not found.", null);
        }

        return (true, StatusCodes.Status200OK, "Success", MapToResponse(category));
    }

    public async Task<(bool Success, int StatusCode, string Message, CategoryResponseDto? Data)> CreateAsync(CreateCategoryDto dto)
    {
        var trimmedName = dto.Name.Trim();
        var trimmedType = dto.Type.Trim();

        var alreadyExists = await categoryRepository.ExistsByNameAndTypeAsync(trimmedName, trimmedType);
        if (alreadyExists)
        {
            return (false, StatusCodes.Status409Conflict, "A category with the same name and type already exists.", null);
        }

        var category = new Category
        {
            Name = trimmedName,
            Type = trimmedType,
            IsActive = true
        };

        var createdCategory = await categoryRepository.AddAsync(category);
        return (true, StatusCodes.Status201Created, "Category created successfully.", MapToResponse(createdCategory));
    }

    public async Task<(bool Success, int StatusCode, string Message, CategoryResponseDto? Data)> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var existingCategory = await categoryRepository.GetActiveByIdAsync(id);
        if (existingCategory is null)
        {
            return (false, StatusCodes.Status404NotFound, "Category not found.", null);
        }

        var trimmedName = dto.Name.Trim();
        var trimmedType = dto.Type.Trim();

        var alreadyExists = await categoryRepository.ExistsByNameAndTypeAsync(trimmedName, trimmedType, id);
        if (alreadyExists)
        {
            return (false, StatusCodes.Status409Conflict, "A category with the same name and type already exists.", null);
        }

        existingCategory.Name = trimmedName;
        existingCategory.Type = trimmedType;

        await categoryRepository.UpdateAsync(existingCategory);
        return (true, StatusCodes.Status200OK, "Category updated successfully.", MapToResponse(existingCategory));
    }

    public async Task<(bool Success, int StatusCode, string Message)> DeleteAsync(int id)
    {
        var existingCategory = await categoryRepository.GetActiveByIdAsync(id);
        if (existingCategory is null)
        {
            return (false, StatusCodes.Status404NotFound, "Category not found.");
        }

        existingCategory.IsActive = false;
        await categoryRepository.UpdateAsync(existingCategory);
        return (true, StatusCodes.Status200OK, "Category deleted successfully.");
    }

    private static CategoryResponseDto MapToResponse(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            IsActive = category.IsActive
        };
    }
}
