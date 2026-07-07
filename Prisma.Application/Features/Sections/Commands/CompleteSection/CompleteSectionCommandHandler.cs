using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Sections;

namespace Prisma.Application.Features.Sections.Commands.CompleteSection;

public class CompleteSectionCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<CompleteSectionCommand>
{
    public async Task Handle(CompleteSectionCommand request, CancellationToken cancellationToken)
    {
        var studentId = currentUserService.UserId;
        if (studentId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var progressRepo = unitOfWork.GetOrCreateRepository<SectionProgress, int>();

        var progress = await progressRepo.FirstOrDefaultAsync(
            new SectionProgressByStudentAndSectionSpecification(studentId.Value, request.SectionId),
            cancellationToken);

        if (progress is null)
            throw new NotFoundException(nameof(SectionProgress), request.SectionId);

        progress.IsCompleted = true;
        progress.Percentage = 100;

        progressRepo.Update(progress);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}