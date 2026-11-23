using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Bookmarks.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications;

namespace Prisma.Application.Features.Bookmarks.Queries.GetSectionBookmarks;

public sealed class GetSectionBookmarksQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IQueryHandler<GetSectionBookmarksQuery, Result<List<BookmarkDto>>>
{
    public async Task<Result<List<BookmarkDto>>> Handle(
        GetSectionBookmarksQuery request,
        CancellationToken ct
    )
    {
        var studentId = currentUserService.UserId;

        if (studentId is null)
            return Result<List<BookmarkDto>>.Unauthorized();

        var bookmarks = await unitOfWork
            .GetOrCreateRepository<Bookmark, int>()
            .ListAsync(
                new BookmarkWithProjectionSpec<BookmarkDto>(
                    studentId.Value,
                    request.SectionId,
                    b => new BookmarkDto(b.PublicId, b.PositionSeconds, b.Note, b.CreatedAt.Value)
                ),
                ct
            );

        return Result.Success(bookmarks.ToList());
    }
}