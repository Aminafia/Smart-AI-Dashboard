namespace Core.AI.Prompts;

public static class PromptTemplate
{
    public static string Analysis(string input)
    {
        return $"""
        You are an AI assistant.
        Analyze the following input and return structured insights.

        Input:
        {input}
        """;
    }
}
