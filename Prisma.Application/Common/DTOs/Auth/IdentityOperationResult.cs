namespace Prisma.Application.Common.DTOs.Auth;

public class IdentityOperationResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static IdentityOperationResult Success => new() { Succeeded = true };

    public static IdentityOperationResult Failed(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };
}