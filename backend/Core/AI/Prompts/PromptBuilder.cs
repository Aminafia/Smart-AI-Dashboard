using Core.AI.Models;

namespace Core.AI.Prompts;

public static class PromptBuilder
{
    public static string Build(AIRequest request)
    {
        return request.TaskType switch
        {
            "analysis" => PromptTemplate.Analysis(request.InputText),
            _ => throw new NotSupportedException("Task type not supported")
        };
    }
}
