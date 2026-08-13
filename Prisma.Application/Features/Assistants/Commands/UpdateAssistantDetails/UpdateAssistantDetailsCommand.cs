using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Assistants.Commands.UpdateAssistantDetails;

public record UpdateAssistantDetailsCommand(
    Guid Id,
    string FirstName,
    string SecondName,
    string Email,
    string PhoneNumber,
    string? Password,
    string[] Policies
) : IRequest<Result<UpdateAssistantDetailsCommandResponse>>;

public record UpdateAssistantDetailsCommandResponse(
Guid Id,
string Email,
string FirstName,
string SecondName,
string PhoneNumber,
string[] Policies);