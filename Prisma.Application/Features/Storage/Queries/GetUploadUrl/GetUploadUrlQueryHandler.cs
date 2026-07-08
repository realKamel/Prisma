using MediatR;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Storage.Queries.GetUploadUrl;

public class GetUploadUrlQueryHandler(IVideoStorageService videoStorage) : IRequestHandler<GetUploadUrlQuery, VideoUploadResult>
{
    public async Task<VideoUploadResult> Handle(GetUploadUrlQuery request, CancellationToken cancellationToken)
    {
        return await videoStorage.GetUploadUrlAsync(request.SectionId, cancellationToken);
    }
}