using MediatR;

namespace Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(string Email) : IRequest<Guid>;
