using Application.Interfaces;

using Infrastructure.AI;
using Infrastructure.AI.Providers;
using Infrastructure.Auth;
using Infrastructure.BackgroundServices;
using Infrastructure.Data;
using Infrastructure.DocumentProcessing;
using Infrastructure.Repositories;
using Infrastructure.Resilience;
using Infrastructure.Services;
using Infrastructure.Storage;

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
        services.AddHttpContextAccessor();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IDocumentStorage, LocalDocumentStorage>();
        services.AddScoped<IDocumentTextExtractor, PdfTextExtractor>();

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentContentRepository, DocumentContentRepository>();

        services.AddScoped<IAIProvider, GeminiProvider>();
        services.AddScoped<IAIService, AiService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICacheService, CacheService>();

        services.AddHttpClient("AIClient")
            .AddPolicyHandler(AIResiliencePolicy.GetRetryPolicy())
            .AddPolicyHandler(AIResiliencePolicy.GetCircuitBreakerPolicy())
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(60));

        services.AddSingleton<IAIQueue, AIQueue>();
        services.AddScoped<IAIJobStore, AIJobStore>();
        services.AddHostedService<AIWorker>();

        return services;
    }
}
