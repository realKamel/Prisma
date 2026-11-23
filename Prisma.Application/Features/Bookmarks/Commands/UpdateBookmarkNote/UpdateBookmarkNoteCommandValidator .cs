using FluentValidation;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Features.Bookmarks.Commands.UpdateBookmarkNote;

public sealed class UpdateBookmarkNoteCommandValidator : AbstractValidator<UpdateBookmarkNoteCommand>
{
    public UpdateBookmarkNoteCommandValidator()
    {
        RuleFor(x => x.BookmarkId).NotEmpty();
        RuleFor(x => x.Note)
            .Must(n => n is null || n.Trim().Length <= Bookmark.MaxNoteLength)
            .WithMessage($"Note cannot exceed {Bookmark.MaxNoteLength} characters.");
    }
}