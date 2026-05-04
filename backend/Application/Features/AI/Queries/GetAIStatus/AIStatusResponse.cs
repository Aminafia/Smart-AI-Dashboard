namespace Application.Features.AI.Queries.GetAIStatus;

public class AIStatusResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? Error { get; set; }
}