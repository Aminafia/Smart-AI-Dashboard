/*
    Secret → used for signing token
    Issuer → who created token
    Audience → who can use token
    ExpiryMinutes → token lifetime
*/
namespace Infrastructure.Auth;

/// <summary>
/// Configuration values for JWT.
/// Loaded from appsettings.json.
/// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiryMinutes { get; set; }
}
