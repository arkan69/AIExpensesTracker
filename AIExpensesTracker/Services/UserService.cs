using ExpenseApi.DTOs.Users;
using ExpenseApi.Entities;
using ExpenseApi.Repositories;
using Microsoft.AspNetCore.Http;

namespace ExpenseApi.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<(bool Success, int StatusCode, string Message, List<UserResponseDto>? Data)> GetAllAsync()
    {
        var users = await userRepository.GetActiveAsync();
        if (users.Count == 0)
        {
            return (false, StatusCodes.Status404NotFound, "No users found.", null);
        }

        return (true, StatusCodes.Status200OK, "Success", users.Select(MapToResponse).ToList());
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await userRepository.GetActiveByIdAsync(id);
        return user is null ? null : MapToResponse(user);
    }

    public async Task<(bool Success, int StatusCode, string Message, UserResponseDto? Data)> CreateAsync(CreateUserDto dto)
    {
        var trimmedName = dto.Name.Trim();
        var trimmedEmail = dto.Email.Trim();

        var emailExists = await userRepository.ExistsByEmailAsync(trimmedEmail);
        if (emailExists)
        {
            return (false, StatusCodes.Status409Conflict, "A user with the same email already exists.", null);
        }

        var user = new User
        {
            Name = trimmedName,
            Email = trimmedEmail,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await userRepository.AddAsync(user);
        return (true, StatusCodes.Status201Created, "User created successfully.", MapToResponse(createdUser));
    }

    public async Task<(bool Success, int StatusCode, string Message, UserResponseDto? Data)> UpdateAsync(int id, UpdateUserDto dto)
    {
        var existingUser = await userRepository.GetActiveByIdAsync(id);
        if (existingUser is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.", null);
        }

        var trimmedName = dto.Name.Trim();
        var trimmedEmail = dto.Email.Trim();

        var emailExists = await userRepository.ExistsByEmailAsync(trimmedEmail, id);
        if (emailExists)
        {
            return (false, StatusCodes.Status409Conflict, "A user with the same email already exists.", null);
        }

        existingUser.Name = trimmedName;
        existingUser.Email = trimmedEmail;

        await userRepository.UpdateAsync(existingUser);
        return (true, StatusCodes.Status200OK, "User updated successfully.", MapToResponse(existingUser));
    }

    public async Task<(bool Success, int StatusCode, string Message)> DeleteAsync(int id)
    {
        var existingUser = await userRepository.GetActiveByIdAsync(id);
        if (existingUser is null)
        {
            return (false, StatusCodes.Status404NotFound, "User not found.");
        }

        existingUser.IsActive = false;
        await userRepository.UpdateAsync(existingUser);

        return (true, StatusCodes.Status200OK, "User deleted successfully.");
    }

    private static UserResponseDto MapToResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
