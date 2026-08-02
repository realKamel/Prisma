using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<List<UserListItemDto>>>;