using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Auth;
using Infrastructure.AI;
using Infrastructure.AI.Providers;
using Application.Interfaces;
using Infrastructure.Services;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        
        // AI Services 
        services.AddScoped<IAIProvider, OpenAIProvider>();
        services.AddScoped<IAIService, AiService>();
        
        // Authentication
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Caching
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }
}