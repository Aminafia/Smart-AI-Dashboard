using MediatR;
using Application.Features.AI.Commands.GenerateAI;

namespace Application.Features.AI.Commands.GenerateAI;
public class GenerateAICommand : IRequest<GenerateAIResponse>
{
    public string Prompt { get; set; } = string.Empty;
}