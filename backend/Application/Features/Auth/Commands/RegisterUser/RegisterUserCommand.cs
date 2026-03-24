using MediatR;

namespace Application.Features.Auth.Commands.RegisterUser;
public class RegisterUserCommand : IRequest<string>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Username { get; set; }
}