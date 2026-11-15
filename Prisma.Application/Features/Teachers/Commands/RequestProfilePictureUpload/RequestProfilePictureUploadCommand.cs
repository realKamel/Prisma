using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Teachers.Commands.RequestProfilePictureUpload;

public sealed record RequestProfilePictureUploadCommand(string FileName, string ContentType, long Size)
    : ICommand<Result<RequestProfilePictureUploadResponse>>,
        IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } = [AppRoles.Teacher];

    public IReadOnlyList<string> RequiredPermissions { get; } = [];
}

public sealed record RequestProfilePictureUploadResponse(Guid FileId, string UploadUrl);
