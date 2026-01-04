using Application.DTOs.Users;
using Application.Services.Interfaces;
using Core.Entities;
using Infrastructure.Data;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public UserResponse CreateUser(CreateUserRequest request)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            CreatedAt = user.CreatedAt
        };
    }

    public List<UserResponse> GetAllUsers()
    {
        return _context.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                CreatedAt = u.CreatedAt
            })
            .ToList();
    }
}