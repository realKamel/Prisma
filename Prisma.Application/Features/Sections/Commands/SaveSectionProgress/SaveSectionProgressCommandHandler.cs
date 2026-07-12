using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Sections;
namespace Prisma.Application.Features.Sections.Commands.SaveSectionProgress;
public class SaveSectionProgressCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<SaveSectionProgressCommand>
{
    public async Task Handle(SaveSectionProgressCommand request, CancellationToken cancellationToken)
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

        var sectionRepo = unitOfWork.GetOrCreateRepository<Section, int>();
        var section = await sectionRepo.GetByIdAsync(request.SectionId, cancellationToken);
        if (section is null)
            throw new NotFoundException(nameof(Section), request.SectionId);

        progress.WatchedSeconds = request.WatchedSeconds;
        progress.Percentage = (int)(request.WatchedSeconds / section.Duration.TotalSeconds * 100);

        progressRepo.Update(progress);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}