using MediatR;
using Prisma.Application.Abstractions.Services;
namespace Prisma.Application.Features.Storage.Queries.GetAudioUrl;

public class GetAudioUrlQueryHandler(IVideoStorageService videoStorage) : IRequestHandler<GetAudioUrlQuery, string>
{
    public async Task<string> Handle(GetAudioUrlQuery request, CancellationToken cancellationToken)
    {
        return await videoStorage.GetAudioUrlAsync(request.ObjectKey);
    }
}