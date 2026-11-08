using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Lessons.Dtos;
using Prisma.Application.Features.Lessons.Helpers;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Sections;

namespace Prisma.Application.Features.Lessons.Commands.RemoveSection;

internal sealed class RemoveSectionCommandHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ILibraryQueryService libraryQueryService
) : IRequestHandler<RemoveSectionCommand, Result>
{
    private sealed record SectionInfo(int Id, Guid? VideoId);

    public async Task<Result> Handle(
        RemoveSectionCommand request,
        CancellationToken cancellationToken
    )
    {
        if (currentUserService.UserId is not { } userId)
        {
            return Result.Unauthorized();
        }

        var sectionRepo = unitOfWork.GetOrCreateRepository<Section, int>();

        var section = await sectionRepo.FirstOrDefaultAsync(
            new SectionByPublicLessonId(
                publicSectionId: request.SectionId,
                publicLessonId: request.LessonId,
                true
            ),
            cancellationToken
        );

        if (section is null)
        {
            return Result.NotFound($"Section {request.SectionId} isn't found");
        }

        section.RemoveVideo();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }

    private async Task<LibraryMainInfoReadModel?> GetLibraryInfo(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        return currentUserService.Role switch
        {
            AppRoles.Teacher => await libraryQueryService.GetLibraryInfoFromTeacher(
                userId,
                cancellationToken
            ),

            AppRoles.Assistant => await libraryQueryService.GetLibraryInfoFromAssistant(
                userId,
                cancellationToken
            ),

            _ => null,
        };
    }
}
