using MediatR;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Storage.Commands.GetDownloadUrl;

public class GetDownloadUrlQueryHandler(IStorageService storage) : IRequestHandler<GetDownloadUrlQuery, string>
{
    public async Task<string> Handle(GetDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        return await storage.GetDownloadUrlAsync(request.BucketName, request.ObjectKey, request.ExpiryMinutes);
    }
}