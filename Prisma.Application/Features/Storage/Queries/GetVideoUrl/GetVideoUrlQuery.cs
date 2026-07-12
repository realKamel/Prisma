using MediatR;
namespace Prisma.Application.Features.Storage.Queries.GetVideoUrl;

public record GetVideoUrlQuery(string ObjectKey) : IRequest<string>;