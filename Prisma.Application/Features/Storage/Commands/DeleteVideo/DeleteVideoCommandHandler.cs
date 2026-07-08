using MediatR;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Storage.Commands.DeleteVideo;
public class DeleteVideoCommandHandler(IVideoStorageService videoStorage) : IRequestHandler<DeleteVideoCommand>
{
    public async Task Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
    {
        await videoStorage.DeleteVideoAsync(request.ObjectKey, cancellationToken);
    }
}