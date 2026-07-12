using MediatR;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Assistants.Queries.GetPolices;

public class GetPolicesQueryHandler : IRequestHandler<GetPolicesQuery, Result<string[]>>
{
    public async Task<Result<string[]>> Handle(GetPolicesQuery request, CancellationToken cancellationToken)
    {
        return AppClaims.Policies.PermissionMap.Keys.ToArray();
    }
}