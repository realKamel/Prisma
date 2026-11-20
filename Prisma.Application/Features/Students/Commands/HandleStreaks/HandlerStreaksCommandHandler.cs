using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Time;
using Prisma.Application.Features.Students.Common;
using Prisma.Application.Features.Students.Queries.GetStreakTasks;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Students.Commands.HandleStreaks;

public sealed class HandlerStreaksCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ISender sender
) : ICommandHandler<HandlerStreaksCommand, Result>
{
    public async Task<Result> Handle(
        HandlerStreaksCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            return Result.Unauthorized();

        var student = await unitOfWork
            .GetOrCreateRepository<Student, Guid>()
            .GetByIdAsync(userId.Value, cancellationToken);

        if (student is null)
            return Result.NotFound();

        var today = CairoDate.Today();

        if (student.LastStreakDate == today)
            return Result.Success(); 

        var tasks = await sender.Send(new GetStreakTasksQuery(), cancellationToken);
        if (!tasks.IsSuccess || tasks.Value.Tasks.Count > 0)
            return Result.Success();

        student.RegisterActivity(today);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}