/*
AiService Working:
1. Recieves AIRequest from IAIService
2. Generate cache key using JobType and Input
3. Try Redis cache for existing result (Fault-tolerant)
4. If cache misses, build prompt based on JobType (GenerateText or Summarize)
5. Call IAIProvider.GenerateAsync() to get result from AI provider
6. Store successful response in Redis (Fault-tolerant + no fallback caching)
7. Return AIProviderResponse containing Output and IsFallback
*/


using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Core.Enums;

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

    public async Task<AIProviderResponse> ProcessAsync(AIRequest request)
    {
        var cacheKey = $"ai:{request.JobType}:{request.Input}";

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
            return new AIProviderResponse
            {
                Output = cachedResult,
                IsFallback = false
            };
        }

        _logger.LogInformation("Cache MISS for key {Key}", cacheKey);

        string prompt = request.JobType switch
        {
            AIJobType.GenerateText => request.Input,

            AIJobType.Summarize =>
                $"Summarize the following text:\n\n{request.Input}",

            _ => throw new NotSupportedException(
                $"Unsupported task {request.JobType}")
        };

        // 2. Call actual AI provider
        var aiResult = await _provider.GenerateAsync(prompt);

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

        return new AIProviderResponse
        {
            Output = aiResult,
            IsFallback = aiResult.StartsWith("⚠️")
        };
    }
}