using Core.Entities;

namespace Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);

        Task AddAsync(User user);

        Task<bool> EmailExistsAsync(string email);
    }
}
