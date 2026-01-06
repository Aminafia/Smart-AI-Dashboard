namespace Application.Users.CreateUser
{
    public class CreateUserResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string Role { get; init; } = default!;
        public DateTime CreatedAt { get; init; }
    }
}
