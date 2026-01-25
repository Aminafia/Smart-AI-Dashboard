using Core.AI.Interfaces;
using Core.AI.Models;
using Core.AI.Prompts;

namespace Core.AI.Services;

public class AIService : IAIService
{
    private readonly IAIProvider _provider;

    public AIService(IAIProvider provider)
    {
        _provider = provider;
    }

    public async Task<AIResponse> AnalyzeAsync(AIRequest request)
    {
        var prompt = PromptBuilder.Build(request);
        var result = await _provider.ExecuteAsync(prompt);

        return new AIResponse
        {
            Result = result,
            Success = true
        };
    }
}
