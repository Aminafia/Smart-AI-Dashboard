namespace Application.Interfaces;

/// <summary>
/// Contract for JWT token generation.
/// Used to decouple Application from Infrastructure.
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(Core.Entities.User user);
}