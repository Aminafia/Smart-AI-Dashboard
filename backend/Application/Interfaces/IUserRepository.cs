using Core.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);    
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

}
