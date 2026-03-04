namespace Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Core.Entities.User user);
}