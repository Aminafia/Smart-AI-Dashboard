/*
 - Commands represents the actions user wants to do 
 - They only represent user entered data, no business logic
 - CommandHandlers are responsible for executing the business logic and returning a response
*/

using MediatR;
using Application.DTOs.AI;

namespace Application.Features.AI.Commands.GenerateText;
public class GenerateTextCommand : IRequest<AIOperationResponse>
{
    public string Prompt { get; set; } = string.Empty;
}