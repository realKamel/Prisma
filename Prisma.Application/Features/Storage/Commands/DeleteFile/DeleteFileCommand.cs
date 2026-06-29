using MediatR;

namespace Prisma.Application.Features.Storage.Commands.DeleteFile;
public record DeleteFileCommand(string BucketName, string ObjectKey) : IRequest<string>;