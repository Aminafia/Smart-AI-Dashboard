/*
AIOperationResponse Purpose:
- Standard response returned to the frontend.
- Shared response model for all AI operations.
- Can represent both asynchronous and synchronous AI operations.
*/


using Core.Enums;

namespace Application.DTOs.AI;

public class AIOperationResponse
{
    public Guid? JobId { get; set; }

    public AIJobStatus Status { get; set; }

    public string Output { get; set; } = string.Empty;
    public bool IsFallback { get; set; }
}