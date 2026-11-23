using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Features.Bookmarks.Common;

public sealed record BookmarkDto(Guid Id, int PositionSeconds, string? Note, DateTimeOffset CreatedAt)
{
    public static BookmarkDto From(Bookmark b) =>
        new(b.PublicId, b.PositionSeconds, b.Note, b.CreatedAt!.Value);
}