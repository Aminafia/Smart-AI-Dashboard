namespace Application.Features.Users.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Role { get; set; } = default!;
}