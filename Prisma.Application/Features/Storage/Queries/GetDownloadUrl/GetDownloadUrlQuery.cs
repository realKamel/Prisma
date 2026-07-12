using MediatR;
namespace Prisma.Application.Features.Storage.Queries.GetDownloadUrl;

public record GetDownloadUrlQuery(string BucketName, string ObjectKey, int ExpiryMinutes = 60) : IRequest<string>;