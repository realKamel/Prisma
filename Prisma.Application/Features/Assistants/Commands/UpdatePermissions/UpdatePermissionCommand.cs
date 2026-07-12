using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Assistants.Dtos;

namespace Prisma.Application.Features.Assistants.Commands.UpdatePermissions;

public record UpdatePermissionCommand(Guid Id, List<string> Permission)
    : IRequest<Result<CreateOrUpdatedAssistantCommandResponse>>;