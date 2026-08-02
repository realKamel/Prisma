using MediatR;
using Ardalis.Result;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Sections;

namespace Prisma.Application.Features.Sections.Commands.CompleteSection;

public class CompleteSectionCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<CompleteSectionCommand, Result>
{
    public async Task<Result> Handle(CompleteSectionCommand request, CancellationToken cancellationToken)
    {
        var studentId = currentUserService.UserId;
        if (studentId is null)
            return Result.Unauthorized("User must be authenticated.");

        var progressRepo = unitOfWork.GetOrCreateRepository<SectionProgress, int>();

        var progress = await progressRepo.FirstOrDefaultAsync(
            new SectionProgressByStudentAndSectionSpecification(studentId.Value, request.SectionId),
            cancellationToken);

        if (progress is null)
            return Result.NotFound($"SectionProgress with id '{request.SectionId}' was not found");

        progress.IsCompleted = true;
        progress.Percentage = 100;

        progressRepo.Update(progress);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}