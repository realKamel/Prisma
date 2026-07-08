using Prisma.Domain.Entities.LessonAggregate;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Sections;

namespace Prisma.Application.Features.Sections.Commands.CreateSectionProgress;
public class CreateSectionProgressCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<CreateSectionProgressCommand>
{
    public async Task Handle(CreateSectionProgressCommand request, CancellationToken cancellationToken)
    {
        var studentId = currentUserService.UserId;
        if (studentId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var progressRepo = unitOfWork.GetOrCreateRepository<SectionProgress, int>();

        var exists = await progressRepo.AnyAsync(
            new SectionProgressByStudentAndSectionSpecification(studentId.Value, request.SectionId),
            cancellationToken);

        if (exists) return;

        progressRepo.Add(new SectionProgress
        {
            SectionId = request.SectionId,
            StudentId = studentId.Value,
            IsCompleted = false,
            WatchedSeconds = 0,
            Percentage = 0
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

