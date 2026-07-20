using MediatR;
using Application.DTOs.AI;

namespace Application.Features.AI.Commands.Summarize
{
public class SummarizeCommand : IRequest<AIOperationResponse>
{
    public string Text { get; set; } = string.Empty;
}
}