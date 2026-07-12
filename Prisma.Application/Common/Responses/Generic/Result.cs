namespace Prisma.Application.Common.Responses.Generic;

public sealed class Result<T> : Result
{
    public T Data { get; private init; }
    public object? Meta { get; private init; }

    private Result() : base()
    {
    }

    public static Result<T> Success(T data, string message = "Success", object? meta = null) =>
        new()
        {
            Succeeded = true,
            Data = data,
            Message = message,
            Meta = meta,
            Errors = null,
        };

    public static new Result<T> Failure(string message) =>
        new() { Succeeded = false, Message = message, Errors = null, Data = default! };

    public static implicit operator Result<T>(T value) => Success(value);
}