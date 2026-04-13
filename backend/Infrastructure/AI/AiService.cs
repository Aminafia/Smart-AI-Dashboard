using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AI;

public class AiService : IAIService
{
    private readonly IAIProvider _provider;
    private readonly ICacheService _cache;
    private readonly ILogger<AiService> _logger;

    public AiService(
        IAIProvider provider, 
        ICacheService cache,
        ILogger<AiService> logger)
    {
        _provider = provider;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AIResponse> GenerateAsync(AIRequest request)
{
    var cacheKey = $"ai:{request.Prompt}";

    // 1. Try cache
    var cachedResult = await _cache.GetAsync(cacheKey);

    if (cachedResult != null)
    {
        _logger.LogInformation("Cache HIT for key {Key}", cacheKey);

        return new AIResponse
        {
            Output = cachedResult
        };
    }

    _logger.LogInformation("Cache MISS for key {Key}", cacheKey);

    // 2. Call actual AI provider (IMPORTANT)
    var aiResult = await _provider.GenerateAsync(request.Prompt);

    // 3. Store in cache
    await _cache.SetAsync(cacheKey, aiResult, TimeSpan.FromMinutes(10));

    _logger.LogInformation("Cache SET for key {Key}", cacheKey);
    
    await Task.Delay(300);

    return new AIResponse
    {
        Output = aiResult
    };
}
}