namespace Prisma.Application.Features.Chats.Dtos;

/// <summary>
/// Timestamp-based cursor page used for message history.
/// </summary>
public sealed record CursorPagedResult<T>(
    IReadOnlyList<T> Items,
    DateTimeOffset? NextCursor,
    bool HasMore
);