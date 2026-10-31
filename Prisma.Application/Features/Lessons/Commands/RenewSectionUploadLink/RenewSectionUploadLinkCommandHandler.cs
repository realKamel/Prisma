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
using Prisma.Domain.Specifications.Sections;
using Prisma.Domain.Specifications.VideoLibrarySpecs;

namespace Prisma.Application.Features.Lessons.Commands.RenewSectionUploadLink;

internal sealed class RenewSectionUploadLinkCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IVideoStreamServices videoStreamServices,
    ISecretCipher secretCipher,
    ILibraryQueryService libraryQueryService
) : IRequestHandler<RenewSectionUploadLinkCommand, Result<RenewSectionUploadLinkResponse>>
{
    private sealed record SectionInfo(int Id, string Title, Guid? VideoId);

    public async Task<Result<RenewSectionUploadLinkResponse>> Handle(
        RenewSectionUploadLinkCommand request,
        CancellationToken cancellationToken
    )
    {
        if (currentUserService.UserId is not { } userId)
        {
            return Result.Unauthorized();
        }

        var library = await GetLibraryInfo(userId, cancellationToken);

        if (library is null)
        {
            return Result.NotFound("The user's library isn't found.");
        }

        if (
            library.ProviderLibraryId is not { } providerLibraryId
            || string.IsNullOrWhiteSpace(library.LibraryApiKey)
        )
        {
            return Result.Error("The user's library isn't active yet. Try again later");
        }

        var sectionRepo = unitOfWork.GetOrCreateRepository<Section, int>();

        var sectionInfo = await sectionRepo.FirstOrDefaultAsync(
            new SectionByPublicLessonIdWithProjection<SectionInfo?>(
                publicSectionId: request.SectionId,
                publicLessonId: request.LessonId,
                (x) =>
                    new SectionInfo(x.Id, x.Title, x.Video != null ? x.Video.ProviderVideoId : null)
            ),
            cancellationToken
        );

        if (sectionInfo is null)
        {
            return Result.NotFound($"Section {request.SectionId} isn't found");
        }

        Guid videoId;

        if (!sectionInfo.VideoId.HasValue)
        {
            var videoRepository = unitOfWork.GetOrCreateRepository<Video, int>();

            var createdVideoResult = await videoStreamServices.CreateVideoAsync(
                providerLibraryId,
                library.LibraryApiKey,
                sectionInfo.Title,
                cancellationToken
            );

            if (!createdVideoResult.IsSuccess)
            {
                return Result.Invalid(createdVideoResult.ValidationErrors);
            }

            var video = createdVideoResult.Value;

            video.AttachToLibrary(library.VideoLibraryId);
            video.AttachToSection(sectionInfo.Id);
            videoRepository.Add(video);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            videoId = video.ProviderVideoId;
        }
        else
        {
            videoId = sectionInfo.VideoId.Value;
        }

        var decryptedLibraryApi = secretCipher.Decrypt(library.LibraryApiKey);

        var expiryTime = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

        var signature = TusUploadSigner.GenerateUploadSignature(
            libraryId: providerLibraryId,
            apiKey: decryptedLibraryApi,
            expirationTime: expiryTime,
            videoId: videoId
        );

        return new RenewSectionUploadLinkResponse(
            SectionId: request.SectionId,
            VideoId: videoId,
            LibraryId: library.ProviderLibraryId.Value,
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
}
