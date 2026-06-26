using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistants;

public record GetAssistantQuery() : IRequest<Result<List<AssistantInfo>>>;

public record AssistantInfo(
    Guid? Id,
    string? Email,
    string? FirstName,
    string? SecondName,
    string? PhoneNumber,
    string[] Policies);