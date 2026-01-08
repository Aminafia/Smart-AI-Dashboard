using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    public Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = new List<UserDto>
        {
            new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com" //default user for demonstration
            }
        };

        return Task.FromResult(users);
    }
}
