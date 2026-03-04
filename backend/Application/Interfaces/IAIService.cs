using Application.DTOs.AI;

namespace Application.Interfaces;

public interface IAIService
{
    Task<AIResponse> GenerateAsync(AIRequest request);
}
