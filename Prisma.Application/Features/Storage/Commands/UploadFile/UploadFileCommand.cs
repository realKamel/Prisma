using MediatR;
namespace Prisma.Application.Features.Storage.Commands.UploadFile;

public record UploadFileCommand(string BucketName, string ObjectKey, Stream Content, string ContentType) : IRequest<string>;