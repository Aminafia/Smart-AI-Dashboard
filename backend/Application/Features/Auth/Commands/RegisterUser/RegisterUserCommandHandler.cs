using MediatR;

namespace Application.Features.Auth.Commands.RegisterUser;
using MediatR;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
{
    public Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {

        //Here it's currently dummy code, later modify and use DuplicateEmailException, BadRequestException for invalid input
        return Task.FromResult("User registered successfully");
    }
}