namespace Prisma.Application.Common.Responses;

public class Result
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string>? Errors { get; init; }

    protected Result() { }

    public static Result Success(string message = "Success") =>
        new() { Succeeded = true, Message = message };

    public static Result Failure(string message, List<string>? errors = null) =>
        new() { Succeeded = false, Message = message, Errors = errors ?? [] };
}