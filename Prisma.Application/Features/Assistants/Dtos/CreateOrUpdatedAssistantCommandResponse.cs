namespace Prisma.Application.Features.Assistants.Dtos;

public record CreateOrUpdatedAssistantCommandResponse(
    Guid Id,
    string FirstName,
    string SecondName,
    string PhoneNumber,
    string Email,
    List<string> Policies);