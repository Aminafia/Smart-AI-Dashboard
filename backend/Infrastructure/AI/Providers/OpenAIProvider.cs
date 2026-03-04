using Application.Interfaces;

namespace Infrastructure.AI.Providers;

public class OpenAIProvider : IAIProvider
{
    public async Task<string> GenerateAsync(string prompt)
    {
        // For now, simple mock response
        // Later we will connect to real OpenAI API

        await Task.Delay(500);

        return $"AI Response to: {prompt}";
    }
}