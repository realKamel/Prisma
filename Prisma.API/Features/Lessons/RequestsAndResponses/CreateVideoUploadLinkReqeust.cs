namespace Prisma.API.Features.Lessons.RequestsAndResponses;

public sealed record CreateSectionAndAttachVideoUploadLinkRequest(
    string Title,
    long VideoDurationInSeconds = 0,
    bool IsPreviewable = false
);
