using Application.DTOs.AI;
using Application.Interfaces;
using MediatR;

namespace Application.Features.AI.Commands.Summarize;

public class SummarizeCommandHandler 
    : IRequestHandler<SummarizeCommand, AIResponse>
{
    private readonly IAIService _aiService;

    public SummarizeCommandHandler(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<AIResponse> Handle(
        SummarizeCommand request,
        CancellationToken cancellationToken)
    {
        var aiRequest = new AIRequest
        {
            Prompt = request.Text
        };

        return await _aiService.GenerateAsync(aiRequest);
    }
}