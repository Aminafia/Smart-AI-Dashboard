using Core.Constants;

namespace Core.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
