using MediatR;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Storage.Commands.DeleteFile;
public class DeleteFileCommandHandler(IStorageService storage) : IRequestHandler<DeleteFileCommand, string>
{
    public async Task<string> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        await storage.DeleteFileAsync(request.BucketName, request.ObjectKey, cancellationToken);
        return "Deleted successfully" ;
    }
}