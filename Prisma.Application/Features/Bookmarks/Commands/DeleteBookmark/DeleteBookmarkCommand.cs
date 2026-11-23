using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Bookmarks.Commands.DeleteBookmark;

public sealed record DeleteBookmarkCommand(Guid BookmarkId) : ICommand<Result>;