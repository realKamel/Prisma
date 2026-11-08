using Ardalis.Result;

namespace Prisma.Application.Abstractions.Security;

public interface IFileScanner
{
    Task<Result> ScanAsync(Stream content, CancellationToken cancellationToken = default);
}
