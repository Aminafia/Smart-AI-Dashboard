using Core.AI.Interfaces;

namespace Core.AI.Providers;

public class OpenAIProvider : IAIProvider
{
    public async Task<string> ExecuteAsync(string prompt)
    {
        // TEMP mock — real API comes later
        await Task.Delay(300);

        return $"[AI Response] Processed prompt: {prompt.Substring(0, Math.Min(100, prompt.Length))}";
    }
}
