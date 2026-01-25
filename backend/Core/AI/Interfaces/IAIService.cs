using Core.AI.Models;

namespace Core.AI.Interfaces;

public interface IAIService
{
    Task<AIResponse> AnalyzeAsync(AIRequest request);
}
