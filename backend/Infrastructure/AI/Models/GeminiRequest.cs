namespace Infrastructure.AI.Models;

public class GeminiRequest
{
    public List<GeminiContent> Contents { get; set; } = new();
}

public class GeminiContent
{
    public List<GeminiPart> Parts { get; set; } = new();
}

public class GeminiPart
{
    public string Text { get; set; } = string.Empty;
}