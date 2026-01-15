using MediatR;
using Core.Entities;
using Core.Interfaces;
using BCrypt.Net;

namespace Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;

    // 🔹 1. Inject repository via constructor
    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // 🔹 2. Real implementation
    public async Task<Guid> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        // Optional but recommended: prevent duplicate emails
        var emailExists = await _userRepository.EmailExistsAsync(request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        // 🔹 3. Create real User entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        // 🔹 4. Persist to DB
        await _userRepository.AddAsync(user);

        // 🔹 5. Return actual ID
        return user.Id;
    }
}
