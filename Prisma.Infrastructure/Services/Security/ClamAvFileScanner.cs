using Ardalis.Result;
using ClamClient.Net.Abstractions;
using ClamClient.Net.Results;
using Microsoft.Extensions.Logging;
using Prisma.Application.Abstractions.Security;

namespace Prisma.Infrastructure.Services.Security;

internal sealed partial class ClamAvFileScanner(
    IClamClient client,
    ILogger<ClamAvFileScanner> logger
) : IFileScanner
{
    public async Task<Result> ScanAsync(
        Stream content,
        CancellationToken cancellationToken = default
    )
    {
        var result = await client.ScanStreamAsync(content, cancellationToken);

        switch (result.Status)
        {
            case ScanStatus.Clean:
                return Result.Success();

            case ScanStatus.ThreatFound:
                MalwareDetected(
                    logger,
                    string.Join(", ", result.Threats.Select(x => x.ThreatName))
                );

                return Result.Error();

            case ScanStatus.StreamTooLarge:
                FileTooLarge(logger);

                return Result.Invalid(new ValidationError("File exceeds the scanning limit."));

            case ScanStatus.Error:
                ScanFailed(logger, result.RawResponse);

                throw new IOException("ClamAV scan failed.");

            default:
                UnexpectedScanStatus(logger, result.Status);

                throw new IOException($"Unexpected ClamAV status: {result.Status}");
        }
    }

    [LoggerMessage(
        EventId = 4011,
        Level = LogLevel.Warning,
        Message = "Malware detected during file scan. Threats: {Threats}"
    )]
    private static partial void MalwareDetected(ILogger logger, string threats);

    [LoggerMessage(
        EventId = 4012,
        Level = LogLevel.Warning,
        Message = "File exceeded the ClamAV scanning size limit"
    )]
    private static partial void FileTooLarge(ILogger logger);

    [LoggerMessage(
        EventId = 4013,
        Level = LogLevel.Error,
        Message = "ClamAV scan failed. Response: {Response}"
    )]
    private static partial void ScanFailed(ILogger logger, string response);

    [LoggerMessage(
        EventId = 4014,
        Level = LogLevel.Error,
        Message = "Unexpected ClamAV scan status: {Status}"
    )]
    private static partial void UnexpectedScanStatus(ILogger logger, ScanStatus status);
}
