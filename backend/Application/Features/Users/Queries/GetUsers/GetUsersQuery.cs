using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<List<UserDto>>;
