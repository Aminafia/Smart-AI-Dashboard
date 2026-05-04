using Core.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);

}
