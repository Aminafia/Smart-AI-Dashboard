/*
    Acts like a "message"
    Controller → Handler
*/

using MediatR;

namespace Application.Features.Auth.Commands.Login;

/// <summary>
/// Represents a login request in the application layer.
/// This command carries user credentials from controller to handler.
/// </summary>
public class LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
