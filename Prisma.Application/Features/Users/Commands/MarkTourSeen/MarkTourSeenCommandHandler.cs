using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Users.Commands.MarkTourSeen;

public sealed class MarkTourSeenCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : ICommandHandler<MarkTourSeenCommand, Result>
{
    public async Task<Result> Handle(MarkTourSeenCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
            return Result.Unauthorized();

        // Loaded as the base type; EF returns the real Student/Teacher/Assistant,
        // so each role's own AllowedTours applies.
        var user = await unitOfWork.GetOrCreateRepository<User, Guid>().GetByIdAsync(userId.Value, ct);

        if (user is null)
            return Result.NotFound();

        var marked = user.MarkTourSeen(request.TourKey);

        if (!marked.IsSuccess)
            return Result.Invalid(marked.ValidationErrors);

        if (!marked.Value)
            return Result.Success(); // already seen, nothing to save

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            // The frontend treats this call as fire-and-forget; the tour just shows once more.
            return Result.Conflict();
        }

        return Result.Success();
    }
}