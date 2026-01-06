using Core.Entities;

namespace Core.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<List<User>> GetAllAsync();
    Task<bool> EmailExistsAsync(string email);
}
