using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Assistants.Dtos;

namespace Prisma.Application.Features.Assistants.Commands.CreateAssistant;

public record CreateAssistantCommand(
    string FirstName,
    string SecondName,
    string Email,
    string PhoneNumber,
    string Password,
    string[] Policies
) : IRequest<Result<CreateOrUpdatedAssistantCommandResponse>>;

