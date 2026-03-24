using MediatR;

namespace Application.Features.Auth.Commands.RegisterUser;
using MediatR;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
{
    public Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult("User registered successfully");
    }
}