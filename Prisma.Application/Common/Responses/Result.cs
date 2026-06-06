namespace Prisma.Application.Common.Responses;

public class Result
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; init; }

    protected Result() { }

    public static Result Success(string message = "Success") =>
        new() { Succeeded = true, Message = message };

    public static Result Failure(string message, Dictionary<string, string[]>? errors = null) =>
        new() { Succeeded = false, Message = message, Errors = errors ?? [] };
}