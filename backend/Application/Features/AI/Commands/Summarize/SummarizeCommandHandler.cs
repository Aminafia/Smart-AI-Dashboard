/*
SummarizeCommandHandler Working:
1. Take SummarizeCommand from controller through MediatR
2. Create new AIRequest with JobType=Summarize
3. Call IAIService.ProcessAsync() with this AIRequest
4. Convert AIProviderResponse -> AIOperationResponse 
5. Return AIOperationResponse to controller
*/

using Application.DTOs.AI;
using Application.Interfaces;
using MediatR;
using Core.Enums;

namespace Application.Features.AI.Commands.Summarize;

public class SummarizeCommandHandler
    : IRequestHandler<SummarizeCommand, AIOperationResponse>
{
    private readonly IAIService _aiService;

    public SummarizeCommandHandler(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<AIOperationResponse> Handle(
        SummarizeCommand request,
        CancellationToken cancellationToken)
    {
        var aiRequest = new AIRequest
        {
            Input = request.Text,
            JobType = AIJobType.Summarize
        };

        var providerResponse = await _aiService.ProcessAsync(aiRequest);

        return new AIOperationResponse
        {
            Status = AIJobStatus.Completed,
            Output = providerResponse.Output,
            IsFallback = providerResponse.IsFallback
        };
    }
}