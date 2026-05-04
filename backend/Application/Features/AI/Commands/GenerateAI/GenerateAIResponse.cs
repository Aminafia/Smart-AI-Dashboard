namespace Application.Features.AI.Commands.GenerateAI;
public class GenerateAIResponse
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
}