using MediatR;
namespace Prisma.Application.Features.Storage.Commands.DeleteVideo;
public record DeleteVideoCommand(string ObjectKey) : IRequest;