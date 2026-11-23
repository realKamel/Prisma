using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications;

namespace Prisma.Application.Features.Bookmarks.Commands.DeleteBookmark;

public sealed class DeleteBookmarkCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : ICommandHandler<DeleteBookmarkCommand, Result>
{
    public async Task<Result> Handle(DeleteBookmarkCommand request, CancellationToken ct)
    {
        var studentId = currentUserService.UserId;

        if (studentId is null)
            return Result.Unauthorized();

        var repository = unitOfWork.GetOrCreateRepository<Bookmark, int>();

        var bookmark = await repository.FirstOrDefaultAsync(
            new BookmarkByPublicIdAndStudentSpecification(request.BookmarkId, studentId.Value),
            ct
        );

        if (bookmark is null)
            return Result.NotFound();

        repository.Delete(bookmark);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}