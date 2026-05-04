using System.Security.Claims;
using System.Text;
using Core.Constants;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

/// <summary>
/// Handles all Authentication & Authorization configuration.
/// Keeps Program.cs clean and modular.
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Registers JWT Authentication and Role-based Authorization policies.
    /// </summary>
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get strongly typed settings
        var jwtSettings = configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>();

        // 🔹 Bind for DI (correct way)
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings")
        );

        // ----------------------
        // Authentication
        // ----------------------
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings!.Issuer,
                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)
                ),

                RoleClaimType = ClaimTypes.Role
            };
        });

        // ----------------------
        // Authorization (Policies)
        // ----------------------
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(Roles.Admin));

            options.AddPolicy("UserOnly", policy =>
                policy.RequireRole(Roles.User));
        });

        return services;
    }
}