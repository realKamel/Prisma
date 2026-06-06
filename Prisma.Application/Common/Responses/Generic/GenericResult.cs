namespace Prisma.Application.Common.Responses.Generic;

public class Result<T> : Result
{
    public T? Data { get; init; }
    public object? Meta { get; init; }

    private Result() { }

    public static Result<T> Success(T data, string message = "Success", object? meta = null) =>
        new() { Succeeded = true, Data = data, Message = message, Meta = meta };

    public static new Result<T> Failure(string message, Dictionary<string, string[]>? errors = null) =>
        new() { Succeeded = false, Message = message, Errors = errors ?? [] };
}