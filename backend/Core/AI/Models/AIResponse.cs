namespace Core.AI.Models;

public class AIResponse
{
    public string Result { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}
