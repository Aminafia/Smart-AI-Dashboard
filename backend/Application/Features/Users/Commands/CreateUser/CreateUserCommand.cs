using MediatR;

namespace Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Email,
    string Name, 
    string Password
) : IRequest<Guid>;
