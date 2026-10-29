using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Lessons.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Domain.Specifications.Sections;
using Prisma.Domain.Specifications.VideoLibrarySpecs;

namespace Prisma.Application.Features.Lessons.Commands.RemoveSection;

internal sealed class RemoveSectionCommandHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork
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
            AppRoles.Teacher => await GetTeacherLibraryInfo(userId, cancellationToken),

            AppRoles.Assistant => await GetAssistantLibraryInfo(userId, cancellationToken),

            _ => null,
        };
    }

    private async Task<LibraryMainInfoReadModel?> GetTeacherLibraryInfo(
        Guid teacherId,
        CancellationToken cancellationToken
    )
    {
        var repository = unitOfWork.GetOrCreateRepository<VideoLibrary, int>();

        var spec = new VideoLibraryByIdWithProjection<LibraryMainInfoReadModel>(
            teacherId,
            library => new LibraryMainInfoReadModel(
                library.Id,
                library.ProviderLibraryId,
                library.LibraryApiKey
            )
        );

        return await repository.FirstOrDefaultAsync(spec, cancellationToken);
    }

    private async Task<LibraryMainInfoReadModel?> GetAssistantLibraryInfo(
        Guid assistantId,
        CancellationToken cancellationToken
    )
    {
        var repository = unitOfWork.GetOrCreateRepository<Assistant, Guid>();

        var spec = new GetAssistantByIdWithProjection<LibraryMainInfoReadModel>(
            assistantId,
            assistant => new LibraryMainInfoReadModel(
                assistant.Teacher.VideoLibrary.Id,
                assistant.Teacher.VideoLibrary.ProviderLibraryId,
                assistant.Teacher.VideoLibrary.LibraryApiKey
            )
        );

        return await repository.FirstOrDefaultAsync(spec, cancellationToken);
    }
}
