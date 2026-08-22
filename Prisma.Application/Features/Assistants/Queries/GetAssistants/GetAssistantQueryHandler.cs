using MediatR;
using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistants;

public class GetAssistantQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetAssistantQuery, Result<List<AssistantInfo>>>
{
    public async Task<Result<List<AssistantInfo>>> Handle(GetAssistantQuery request,
    CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetOrCreateRepository<Assistant, Guid>();
        var teacherId = currentUserService.UserId;
        var dto = await repo.ListAsync(
            new AssistantWithProjectionSpec<AssistantInfo>(teacherId, a => new AssistantInfo(
                a.Id,
                a.Email,
                a.FirstName,
                a.LastName,
                a.PhoneNumber,
                a.Claims
                    .Where(c => c.ClaimValue != null)
                    .Select(c => c.ClaimValue!)
                    .ToArray()
            )),
            cancellationToken);

        return dto.ToList();
    }
}