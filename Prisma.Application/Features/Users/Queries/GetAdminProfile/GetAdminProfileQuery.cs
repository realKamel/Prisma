using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Queries.GetAdminProfile;

public record GetAdminProfileQuery(Guid AdminId) : IRequest<Result<RoleProfileDto>>;