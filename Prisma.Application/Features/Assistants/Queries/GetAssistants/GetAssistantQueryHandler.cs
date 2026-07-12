using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistants;

public class GetAssistantQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAssistantQuery, Result<List<AssistantInfo>>>
{
    public async Task<Result<List<AssistantInfo>>> Handle(GetAssistantQuery request,
        CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetOrCreateRepository<Assistant, Guid>();

        var assistants = await repo.ListAsync(new AssistantsWithClaimsSpec(), cancellationToken);

        List<AssistantInfo> dto = [];

        foreach (var a in assistants)
        {
            dto.Add(new AssistantInfo(
                a.Id, a.Email, a.FirstName, a.LastName,
                a.PhoneNumber,
                (a.Claims
                    .Where(c => c.ClaimValue is not null)
                    .Select(x => x.ClaimValue).ToArray())));
        }

        return dto.ToList();
    }
}