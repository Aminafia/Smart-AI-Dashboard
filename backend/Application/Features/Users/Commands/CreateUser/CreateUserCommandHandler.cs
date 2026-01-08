using MediatR;

namespace Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Guid>
{
    public Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var fakeId = Guid.NewGuid();
        return Task.FromResult(fakeId);
    }
}
