using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Abstractions.Services.VideoServices;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Interfaces;
using Prisma.Application.Features.Lessons.Dtos;
using Prisma.Application.Features.Lessons.Helpers;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.VideoLibrarySpecs;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonSection;

internal sealed class CreateLessonSectionCommandHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IVideoStreamServices streamServices,
    ILibraryQueryService libraryQueryService,
    ISecretCipher secretCipher
)
    : IRequestHandler<
        CreateLessonSectionCommand,
        Result<CreateSectionAndAttachVideoUploadLinkResponse>
    >
{
    private sealed record LessonInfo(int LessonId, IEnumerable<SectionInfo> Sections);

    private sealed record SectionInfo(int SectionId, int SortOrder);

    public async Task<Result<CreateSectionAndAttachVideoUploadLinkResponse>> Handle(
        CreateLessonSectionCommand request,
        CancellationToken cancellationToken
    )
    {
        if (currentUserService.UserId is not { } userId)
        {
            return Result.Unauthorized();
        }

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lesson = await lessonRepo.FirstOrDefaultAsync(
            new LessonByPublicId(request.LessonId, true),
            cancellationToken
        );

        if (lesson is null)
        {
            return Result.NotFound(
                $"Lesson with ID {request.LessonId} was not found, or it has no available sections."
            );
        }

        var library = await GetLibraryInfo(userId, cancellationToken);

        if (library is null)
        {
            return Result.NotFound("Teacher's library isn't found.");
        }

        if (
            library.ProviderLibraryId is not { } providerLibraryId
            || string.IsNullOrWhiteSpace(library.LibraryApiKey)
        )
        {
            return Result.Error("The user's library isn't active yet. Try again later.");
        }

        var decryptedLibraryApiKey = secretCipher.Decrypt(library.LibraryApiKey);

        var createdVideoResult = await streamServices.CreateVideoAsync(
            providerLibraryId,
            decryptedLibraryApiKey,
            request.Title,
            cancellationToken
        );

        if (!createdVideoResult.IsSuccess)
        {
            return createdVideoResult.Map();
        }

        var video = createdVideoResult.Value;

        video.AttachToLibrary(library.VideoLibraryId);

        var sectionResult = await CreateNextSection(request, cancellationToken);

        var section = sectionResult.Value;

        section.AttachVideo(video.Id);

        lesson.AddSection(section);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        //TODO: this must be fixed
        var expiryTime = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

        var signature = TusUploadSigner.GenerateUploadSignature(
            providerLibraryId,
            library.LibraryApiKey,
            expiryTime,
            video.ProviderVideoId
        );

        return new CreateSectionAndAttachVideoUploadLinkResponse(
            SectionId: section.PublicId,
            VideoId: video.ProviderVideoId,
            LibraryId: providerLibraryId,
            ExpirationTime: expiryTime,
            Signature: signature
        );
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

    private async Task<Result<Section>> CreateNextSection(
        CreateLessonSectionCommand request,
        CancellationToken cancellationToken
    )
    {
        var repository = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new LessonByPublicIdWithProjection<LessonInfo>(
            request.LessonId,
            lesson => new LessonInfo(
                lesson.Id,
                lesson
                    .Sections.OrderBy(x => x.CreatedAt)
                    .Select(y => new SectionInfo(y.Id, y.SortOrder))
            )
        );

        var lessonInfo = await repository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lessonInfo is null)
        {
            return Result.NotFound($"Lesson with Id {request.LessonId} isn't found.");
        }

        var sections = lessonInfo.Sections.ToList();

        var sortOrder = sections.LastOrDefault()?.SortOrder ?? 0;

        return Section.Create(
            request.Title,
            lessonInfo.LessonId,
            sortOrder,
            request.IsPreviewable,
            request.VideoDurationInSeconds
        );
    }
}
