using FluentValidation;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Features.Bookmarks.Commands.CreateBookmark;

public sealed class CreateBookmarkCommandValidator : AbstractValidator<CreateBookmarkCommand>
{
    public CreateBookmarkCommandValidator()
    {
        RuleFor(x => x.SectionId).GreaterThan(0);
        RuleFor(x => x.PositionSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Note)
            .Must(n => n is null || n.Trim().Length <= Bookmark.MaxNoteLength)
            .WithMessage($"Note cannot exceed {Bookmark.MaxNoteLength} characters.");
    }
}