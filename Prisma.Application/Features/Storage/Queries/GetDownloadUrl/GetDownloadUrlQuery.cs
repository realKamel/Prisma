using MediatR;
namespace Prisma.Application.Features.Storage.Commands.GetDownloadUrl;

public record GetDownloadUrlQuery(string BucketName, string ObjectKey, int ExpiryMinutes = 60) : IRequest<string>;