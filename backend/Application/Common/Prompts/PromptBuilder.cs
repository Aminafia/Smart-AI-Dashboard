using Application.DTOs.AI;

namespace Application.Common.Prompts;

public static class PromptBuilder
{
    public static string Build(GenerateTextRequest request)
    {
        return request.Prompt;
    }
}