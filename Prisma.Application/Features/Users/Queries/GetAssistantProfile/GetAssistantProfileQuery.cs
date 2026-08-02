using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Queries.GetAssistantProfile;

public record GetAssistantProfileQuery(Guid AssistantId) : IRequest<Result<RoleProfileDto>>;