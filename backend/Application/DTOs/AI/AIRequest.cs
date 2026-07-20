/*
AIRequest Purpose:
- Internal model used by AIService to represent a request for processing a specific job type with given input.
- Every AI operation (e.g., text generation, summarization) is represented as an AIRequest
*/


using Core.Enums;

namespace Application.DTOs.AI;

public class AIRequest
{
    public string Input { get; set; } = string.Empty;

    public AIJobType JobType { get; set; }
}