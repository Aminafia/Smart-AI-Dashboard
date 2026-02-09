using Core.AI.Models;

namespace Core.AI.Interfaces;

public interface IAIService
{
    Task<AIResponse> GenerateAsync(AIRequest request);
}
