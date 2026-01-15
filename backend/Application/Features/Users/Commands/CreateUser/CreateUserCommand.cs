using MediatR;

namespace Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Email,
    string FullName, 
    string Password
) : IRequest<Guid>;
