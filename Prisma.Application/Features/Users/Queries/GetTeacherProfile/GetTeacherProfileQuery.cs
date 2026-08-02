using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Queries.GetTeacherProfile;

public record GetTeacherProfileQuery(Guid TeacherId) : IRequest<Result<RoleProfileDto>>;