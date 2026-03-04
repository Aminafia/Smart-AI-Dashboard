using Application.DTOs.AI;

namespace Application.Common.Prompts;

public static class PromptBuilder
{
    public static string Build(AIRequest request)
    {
        return request.Prompt;
    }
}