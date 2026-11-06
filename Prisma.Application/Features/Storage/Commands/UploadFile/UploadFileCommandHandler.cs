using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services.ObjectStorage;

namespace Prisma.Application.Features.Storage.Commands.UploadFile;

internal sealed class UploadFileCommandHandler(IStorageService storage)
    : IRequestHandler<UploadFileCommand, Result>
{
    public async Task<Result> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        await storage.UploadFileAsync(
            request.BucketName,
            request.ObjectKey,
            request.Content,
            request.ContentType,
            cancellationToken
        );
        return Result.SuccessWithMessage("Uploaded Successfully");
    }
}
