using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Bookmarks.Common;

namespace Prisma.Application.Features.Bookmarks.Commands.UpdateBookmarkNote;

public sealed record UpdateBookmarkNoteCommand(Guid BookmarkId, string? Note)
    : ICommand<Result<BookmarkDto>>;