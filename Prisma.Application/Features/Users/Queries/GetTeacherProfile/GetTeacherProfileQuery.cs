using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Queries.GetTeacherProfile;

public record GetTeacherProfileQuery(Guid TeacherId) : IRequest<Result<RoleProfileDto>>;