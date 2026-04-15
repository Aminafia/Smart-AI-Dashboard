namespace Application.DTOs.AI;

public class AIResponse
{
    public string Output { get; set; } = string.Empty;
    public bool IsFallback { get; set; } = false;
}