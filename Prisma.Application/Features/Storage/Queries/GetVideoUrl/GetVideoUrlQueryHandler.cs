using MediatR;
using Prisma.Application.Abstractions.Services;
namespace Prisma.Application.Features.Storage.Queries.GetVideoUrl;

public class GetVideoUrlQueryHandler(IVideoStorageService videoStorage) : IRequestHandler<GetVideoUrlQuery, string>
{
    public async Task<string> Handle(GetVideoUrlQuery request, CancellationToken cancellationToken)
    {
        return await videoStorage.GetVideoUrlAsync(request.ObjectKey);
    }
}