using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Bookmarks.Common;

namespace Prisma.Application.Features.Bookmarks.Queries.GetSectionBookmarks;

public sealed record GetSectionBookmarksQuery(int SectionId) : IQuery<Result<List<BookmarkDto>>>;