namespace Application.Users.CreateUser
{
    public class CreateUserRequest
    {
        public string Name { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string Role { get; init; } = "Viewer";
    }
}
