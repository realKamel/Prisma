using MediatR;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Storage.Commands.UploadFile;
public class UploadFileCommandHandler(IStorageService storage) : IRequestHandler<UploadFileCommand, string>
{
    public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        await storage.UploadFileAsync(request.BucketName, request.ObjectKey, request.Content, request.ContentType, cancellationToken);
        return "Uploaded Successfully";
    }
}