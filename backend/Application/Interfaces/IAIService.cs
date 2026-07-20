/*
IAIService Purpose:
- This interface defines the contract for AI service implementations.
- It provides a method to process AI requests and return responses.
- Handlers implementing this interface will encapsulate the logic for interacting with AI providers, managing requests, and handling responses.
*/


using Application.DTOs.AI;

namespace Application.Interfaces;

public interface IAIService
{
    Task<AIProviderResponse> ProcessAsync(AIRequest request);
}
