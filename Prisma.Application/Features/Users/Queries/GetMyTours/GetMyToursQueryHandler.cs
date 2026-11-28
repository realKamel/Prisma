using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Users.Queries.GetMyTours;

public sealed class GetMyToursQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IQueryHandler<GetMyToursQuery, Result<IReadOnlyCollection<string>>>
{
    public async Task<Result<IReadOnlyCollection<string>>> Handle(
        GetMyToursQuery request,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
            return Result.Unauthorized();

        var user = await unitOfWork
            .GetOrCreateRepository<User, Guid>()
            .GetByIdAsync(userId.Value, ct);

        if (user is null)
            return Result.NotFound();

        return Result.Success<IReadOnlyCollection<string>>(
            user.SeenTours.ToArray()
        );
    }
}