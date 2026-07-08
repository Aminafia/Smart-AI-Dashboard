/*
    user (input)
    → contains Id, Email, Role

    a. claims
    → created from user

    b. key 
    → created from Secret 
        (_jwtSettings.Secret → converted to bytes → used as signing key)

    c. creds
    → signing credentials created using the key and HMAC SHA256 algorithm.

    token (a + b + c)
    → created using claims + settings
       - issuer → who created token
       - audience → who uses token
       - claims → user data
       - expires → expiry time
       - signature → security

    return string token
*/
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Core.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth;

/// <summary>
/// Generates JWT token using user data and configuration settings.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _jwtSettings = options.Value;
    }

    /// <summary>
    /// 1. Takes `user` as input containing Id, Email, Role.
    /// 2. Creates a. `claims` from user data.
    /// 3. Creates signing b. `key` from `Secret` in `JWTSettings`. 
    ///   - _jwtSettings.Secret → converted to bytes → used as signing key
    /// 4. Creates signing c. `creds` using the key and HMAC SHA256 algorithm.
    /// 5. Creates JWT `token` using claims, settings, and signing credentials.
    /// 6. Returns the generated token as a string.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public string GenerateToken(User user)
    {
var claims = new List<Claim>
{
    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Email, user.Email),
    new Claim("fullName", user.FullName),
    new Claim(ClaimTypes.Role, user.Role)
};

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
