using Application.DTOs.Users;

namespace Aplication.Services.Interfaces;

public interface IUserService
{
    UserResponse CreateUser(CreateUserRequest request);
    List<UserResponse> GetAllUsers();
}
