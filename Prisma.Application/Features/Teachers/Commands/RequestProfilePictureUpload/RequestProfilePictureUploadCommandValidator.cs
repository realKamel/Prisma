using FluentValidation;

namespace Prisma.Application.Features.Teachers.Commands.RequestProfilePictureUpload;

public sealed class RequestProfilePictureUploadCommandValidator
    : AbstractValidator<RequestProfilePictureUploadCommand>
{
    // PNG and WebP keep transparency (cutouts); JPEG is accepted too. Nothing here converts them.
    private static readonly string[] AllowedTypes = ["image/png", "image/webp", "image/jpeg"];

    private const long MaxSizeBytes = 10L * 1024 * 1024;

    public RequestProfilePictureUploadCommandValidator()
    {
        RuleFor(x => x.FileName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("VALIDATION.REQUIRED")
            .MaximumLength(255)
            .WithMessage("VALIDATION.MAX_LENGTH");

        RuleFor(x => x.ContentType)
            .Must(t => t is not null && AllowedTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("TEACHER_PROFILE_PAGE.PICTURE.NOT_IMAGE");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, MaxSizeBytes)
            .WithMessage("TEACHER_PROFILE_PAGE.PICTURE.TOO_LARGE");
    }
}
