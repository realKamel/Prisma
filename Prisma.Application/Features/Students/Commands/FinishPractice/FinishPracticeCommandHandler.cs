using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Time;
using Prisma.Application.Features.Students.Common;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Students.Commands.FinishPractice;

public sealed class FinishPracticeCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : ICommandHandler<FinishPracticeCommand, Result<FinishPracticeResultDto>>
{
    public async Task<Result<FinishPracticeResultDto>> Handle(
        FinishPracticeCommand request,
        CancellationToken ct
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
            return Result<FinishPracticeResultDto>.Unauthorized();

        var student = await unitOfWork
            .GetOrCreateRepository<Student, Guid>()
            .GetByIdAsync(userId.Value, ct);

        if (student is null)
            return Result<FinishPracticeResultDto>.NotFound();

        var today = CairoDate.Today();

        var practiced = student.RegisterPractice(today);
        var streakChanged = student.RegisterActivity(today);
        var counted = false;
        if (practiced || streakChanged)
        {
            try
            {
                await unitOfWork.SaveChangesAsync(ct);
                counted = streakChanged;
            }
            catch (Exception ex)
                when (ex is UniqueConstraintViolationException or ConcurrencyConflictException)
            { }
        }

        return Result.Success(new FinishPracticeResultDto(counted));
    }
}