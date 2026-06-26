using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Assistants.Dtos;

namespace Prisma.Application.Features.Assistants.Commands.CreateAssistant;

public record CreateAssistantCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    string[] Policies
) : IRequest<Result<CreateOrUpdatedAssistantCommandResponse>>;

