using MediatR;
namespace Prisma.Application.Features.Storage.Queries.GetAudioUrl;

public record GetAudioUrlQuery(string ObjectKey) : IRequest<string>;