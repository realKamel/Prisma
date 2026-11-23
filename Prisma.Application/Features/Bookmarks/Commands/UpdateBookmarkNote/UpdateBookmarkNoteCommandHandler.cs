using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Bookmarks.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications;

namespace Prisma.Application.Features.Bookmarks.Commands.UpdateBookmarkNote;

public sealed class UpdateBookmarkNoteCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : ICommandHandler<UpdateBookmarkNoteCommand, Result<BookmarkDto>>
{
    public async Task<Result<BookmarkDto>> Handle(
        UpdateBookmarkNoteCommand request,
        CancellationToken ct
    )
    {
        var studentId = currentUserService.UserId;

        if (studentId is null)
            return Result<BookmarkDto>.Unauthorized();

        var bookmark = await unitOfWork
            .GetOrCreateRepository<Bookmark, int>()
            .FirstOrDefaultAsync(
                new BookmarkByPublicIdAndStudentSpecification(request.BookmarkId, studentId.Value),
                ct
            );

        // Someone else's bookmark looks the same as a missing one
        if (bookmark is null)
            return Result<BookmarkDto>.NotFound();

        var updated = bookmark.UpdateNote(request.Note);

        if (!updated.IsSuccess)
            return Result<BookmarkDto>.Invalid(updated.ValidationErrors);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(BookmarkDto.From(bookmark));
    }
}