using ExpenseApi.DTOs.Users;

namespace ExpenseApi.Services;

public interface IUserService
{
    Task<(bool Success, int StatusCode, string Message, List<UserResponseDto>? Data)> GetAllAsync();

    Task<UserResponseDto?> GetByIdAsync(int id);

    Task<(bool Success, int StatusCode, string Message, UserResponseDto? Data)> CreateAsync(CreateUserDto dto);

    Task<(bool Success, int StatusCode, string Message, UserResponseDto? Data)> UpdateAsync(int id, UpdateUserDto dto);

    Task<(bool Success, int StatusCode, string Message)> DeleteAsync(int id);
}
