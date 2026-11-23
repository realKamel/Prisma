using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Bookmarks.Common;

namespace Prisma.Application.Features.Bookmarks.Commands.CreateBookmark;

public sealed record CreateBookmarkCommand(int SectionId, int PositionSeconds, string? Note)
    : ICommand<Result<BookmarkDto>>;