using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
