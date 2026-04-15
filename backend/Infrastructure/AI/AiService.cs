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
        
        // 1. Try cache (Fault-tolerant)
        string? cachedResult = null;
        try
        {
            cachedResult = await _cache.GetAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache GET failed for key {Key}", cacheKey);
        }
        if (cachedResult != null)
        {
            _logger.LogInformation("Cache HIT for key {Key}", cacheKey);
            return new AIResponse
            {
                Output = cachedResult,
                IsFallback = false
            };
        }

        _logger.LogInformation("Cache MISS for key {Key}", cacheKey);
        
        // 2. Call actual AI provider
        var aiResult = await _provider.GenerateAsync(request.Prompt);
        
        // 3. Store in cache (Fault-tolerant + no fallback caching)
        if (!aiResult.StartsWith("⚠️"))
        {
            try
            {
                await _cache.SetAsync(cacheKey, aiResult, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Cache SET for key {Key}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache SET failed for key {Key}", cacheKey);
            }
        }

        return new AIResponse
        {
            Output = aiResult,
            IsFallback = aiResult.StartsWith("⚠️")
        };
    }
}