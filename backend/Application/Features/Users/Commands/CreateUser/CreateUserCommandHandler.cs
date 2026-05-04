using Application.Common.Exceptions;
using Application.Interfaces;
using BCrypt.Net;
using Core.Constants;
using Core.Entities;
using MediatR;

namespace Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;

    // 1. Inject repository via constructor
    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // 2. Real implementation
    public async Task<Guid> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var emailExists = await _userRepository.EmailExistsAsync(
            request.Email,
            cancellationToken
        );
        if (emailExists)
        {
            throw new DuplicateEmailException(request.Email);
        }

        // 3. Create real User entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.Name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Roles.User,
            CreatedAt = DateTime.UtcNow
        };

        // 4. Persist to DB
        await _userRepository.AddUserAsync(user, cancellationToken);

        // 5. Return actual ID
        return user.Id;
    }
}
