/*
AIProvider Purpose:
- Another Abstraction used by AIService to interact with different AI providers- OpenAIProvider, GeminiProvider.
*/

namespace Application.Interfaces;

public interface IAIProvider
{
    Task<string> GenerateAsync(string prompt);
}