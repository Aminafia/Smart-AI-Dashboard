namespace Application.Features.Auth.Commands.Login;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}