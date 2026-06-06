namespace Prisma.Application.Common.Responses;

public class Result<T>
{
    public bool Succeeded { get; init; }
    public string Message { get; init; }
    public T Data { get; init; }
    public List<string>? Errors { get; init; }
    public object? Meta { get; init; }

    private Result() { }

    public static Result<T> Success(T data, string message = "Success", object meta = null) =>
        new() { Succeeded = true, Data = data, Message = message, Meta = meta };

    public static Result<T> Failure(string message, List<string> errors = null) =>
        new() { Succeeded = false, Message = message, Errors = errors ?? [] };
}