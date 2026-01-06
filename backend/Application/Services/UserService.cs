using Application.DTOs.Users;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new Exception("Email already exists");
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.AddAsync(user);
        
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            Email = u.Email,
            FullName = u.FullName,
            CreatedAt = u.CreatedAt
        }).ToList();
    }
}
