using Ardalis.Result;

namespace Prisma.Application.Abstractions.Services.Security;

public interface IFileScanner
{
    Task<Result> ScanAsync(Stream content, CancellationToken cancellationToken = default);
}
