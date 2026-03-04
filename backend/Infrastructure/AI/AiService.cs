using System.Net.Http.Json;
using Application.DTOs.AI;
using Application.Interfaces;

namespace Infrastructure.AI;

public class AiService : IAIService
{
    private readonly IAIProvider _provider;

    public AiService(IAIProvider provider)
    {
        _provider = provider;
    }

    public async Task<AIResponse> GenerateAsync(AIRequest request)
    {
        var result = await _provider.GenerateAsync(request.Prompt);

        return new AIResponse
        {
            Output = result
        };
    }
}