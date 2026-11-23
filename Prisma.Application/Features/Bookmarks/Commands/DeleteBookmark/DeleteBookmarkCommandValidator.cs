using FluentValidation;

namespace Prisma.Application.Features.Bookmarks.Commands.DeleteBookmark;

public sealed class DeleteBookmarkCommandValidator : AbstractValidator<DeleteBookmarkCommand>
{
    public DeleteBookmarkCommandValidator()
    {
        RuleFor(x => x.BookmarkId).NotEmpty();
    }
}