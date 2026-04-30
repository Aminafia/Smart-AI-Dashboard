using Application.Interfaces;
using Infrastructure.AI;
using Infrastructure.AI.Providers;
using Infrastructure.Auth;
using Infrastructure.BackgroundServices;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Resilience;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

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

        // Resilience
        services.AddHttpClient("AIClient")
            .AddPolicyHandler(AIResiliencePolicy.GetRetryPolicy())
            .AddPolicyHandler(AIResiliencePolicy.GetCircuitBreakerPolicy())
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10));

        // Queue + Job Store
        _ = services.AddSingleton<IAIQueue, AIQueue>();
        services.AddSingleton<IAIJobStore, AIJobStore>();

        // Background Worker
        services.AddHostedService<AIWorker>();

        return services;
    }
}