using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Auth;
using Infrastructure.AI;
using Infrastructure.AI.Providers;
using Application.Interfaces;
using Core.Interfaces;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IAIProvider, OpenAIProvider>();
        services.AddScoped<IAIService, AiService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}