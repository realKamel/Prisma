using FluentValidation;

namespace Prisma.Application.Features.Lessons.Commands.AddLessonCover;

public sealed class AddLessonCoverCommandValidator : AbstractValidator<AddLessonCoverCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    public AddLessonCoverCommandValidator()
    {
        RuleFor(x => x.PublicLessonId).NotEmpty().WithMessage("Lesson ID is required.");

        RuleFor(x => x.FileDto.FileName).NotEmpty().WithMessage("File name is required.");

        RuleFor(x => x.FileDto.ContentType)
            .Must(type => AllowedContentTypes.Contains(type))
            .WithMessage("Only JPEG, PNG, and WebP images are allowed.");

        RuleFor(x => x.FileDto.Size)
            .GreaterThan(0)
            .WithMessage("Image file cannot be empty.")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage("Image size must not exceed 10 MB.");

        RuleFor(x => x.Content)
            .NotNull()
            .WithMessage("Image content is required.")
            .Must(stream => stream.CanRead)
            .When(x => x.Content is not null)
            .WithMessage("Image content must be readable.");
    }
}
