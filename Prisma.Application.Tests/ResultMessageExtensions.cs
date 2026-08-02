using Ardalis.Result;

namespace Prisma.Application.Tests;

/// <summary>
/// Backward-compatible helper that mimics the old custom Result.Message semantics
/// so existing test assertions keep working with Ardalis.Result.
/// On success returns <see cref="IResult.SuccessMessage"/>; on failure returns the
/// first error message (or the messages joined) from <see cref="IResult.Errors"/>.
/// </summary>
public static class ResultMessageExtensions
{
    public static string GetResultMessage(this Result result)
        => result.IsSuccess
            ? result.SuccessMessage ?? string.Empty
            : string.Join(", ", result.Errors);

    public static string GetResultMessage<T>(this Result<T> result)
        => result.IsSuccess
            ? result.SuccessMessage ?? string.Empty
            : string.Join(", ", result.Errors);
}