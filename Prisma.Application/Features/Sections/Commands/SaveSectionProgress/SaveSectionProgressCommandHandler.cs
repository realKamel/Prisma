using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Sections;

namespace Prisma.Application.Features.Sections.Commands.SaveSectionProgress;

public class SaveSectionProgressCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IRequestHandler<SaveSectionProgressCommand, Result>
{
    public async Task<Result> Handle(
        SaveSectionProgressCommand request,
        CancellationToken cancellationToken
    )
    {
        var studentId = currentUserService.UserId;

        if (studentId is null)
        {
            return Result.Unauthorized();
        }

        var progressRepo = unitOfWork.GetOrCreateRepository<SectionProgress, int>();

        var progress = await progressRepo.FirstOrDefaultAsync(
            new SectionProgressByStudentAndSectionSpecification(studentId.Value, request.SectionId),
            cancellationToken
        );

        if (progress is null)
            return Result.NotFound($"SectionProgress with id '{request.SectionId}' was not found");

        var sectionRepo = unitOfWork.GetOrCreateRepository<Section, int>();
        var sectionDuration = await sectionRepo.FirstOrDefaultAsync(
            new SectionWithProjectionSpec<int>(request.SectionId, s => s.TimeDuration.Seconds),
            cancellationToken
        );

        progress.WatchedSeconds = request.WatchedSeconds;

        progress.Percentage = (int)(request.WatchedSeconds / sectionDuration * 100);

        progressRepo.Update(progress);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
