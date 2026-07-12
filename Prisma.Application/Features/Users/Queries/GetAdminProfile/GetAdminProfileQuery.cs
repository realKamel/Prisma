using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Queries.GetAdminProfile;

public record GetAdminProfileQuery(Guid AdminId) : IRequest<Result<RoleProfileDto>>;