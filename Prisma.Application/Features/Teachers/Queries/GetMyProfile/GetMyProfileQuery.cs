using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Teachers.Profile;

namespace Prisma.Application.Features.Teachers.Queries.GetMyProfile;

public sealed record GetMyProfileQuery : IQuery<Result<MyProfileResponse>>, IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } = [AppRoles.Teacher];

    public IReadOnlyList<string> RequiredPermissions { get; } = [];
}
