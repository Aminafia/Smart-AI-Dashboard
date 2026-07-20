/*
AIProviderResponse Purpose:
- Internal response model returned by AIService.
- Contains provider-specific information.
- Not exposed directly to the frontend.
*/

namespace Application.DTOs.AI;
public class AIProviderResponse
{
    public string Output { get; set; } = string.Empty;

    public bool IsFallback { get; set; }
}