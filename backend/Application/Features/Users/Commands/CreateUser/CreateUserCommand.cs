using MediatR;

namespace Application.Features.Users.Commands;

public record CreateUserCommand(
    string Email,
    string FullName
) : IRequest<Guid>;
