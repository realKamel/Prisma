using System.Text.RegularExpressions;
using FluentValidation;
using Prisma.Application.Features.Teachers.Profile;

namespace Prisma.Application.Features.Teachers.Commands.UpdateMyProfile;

/// <summary>
/// Mirrors the Angular form rules. Messages are i18n keys the front already translates.
/// </summary>
public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    private const int SubjectMax = 40;
    private const int BioMax = 600;
    private const int AnnouncementMax = 160;

    private static readonly Regex NamePattern = new(@"^[\p{L}\s'-]+$", RegexOptions.Compiled);

    public UpdateMyProfileCommandValidator()
    {
        NamePart(RuleFor(x => x.FirstName));
        NamePart(RuleFor(x => x.SecondName));
        NamePart(RuleFor(x => x.ThirdName));
        NamePart(RuleFor(x => x.LastName));

        RuleFor(x => x.Subject)
            .Cascade(CascadeMode.Stop)
            .Must(v => !string.IsNullOrWhiteSpace(v))
            .WithMessage("VALIDATION.REQUIRED")
            .Must(v => v.Trim().Length <= SubjectMax)
            .WithMessage("VALIDATION.MAX_LENGTH");

        RuleFor(x => x.Bio)
            .Must(v => v is null || v.Trim().Length <= BioMax)
            .WithMessage("VALIDATION.MAX_LENGTH");

        // A past end date is not an error: the announcement just doesn't show.
        RuleFor(x => x.AnnouncementText)
            .Must(v => v is null || v.Trim().Length <= AnnouncementMax)
            .WithMessage("VALIDATION.MAX_LENGTH");

        RuleFor(x => x.YearsOfExperience)
            .Cascade(CascadeMode.Stop)
            .Must(v => v is null or >= 0)
            .WithMessage("VALIDATION.MIN_VALUE")
            .Must(v => v is null or <= 60)
            .WithMessage("VALIDATION.MAX_VALUE");

        RuleFor(x => x.Color).IsInEnum().WithMessage("VALIDATION.INVALID");

        RuleFor(x => x.Visibility).NotNull().WithMessage("VALIDATION.REQUIRED");

        RuleFor(x => x.Links)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("VALIDATION.REQUIRED")
            .Custom(
                (links, ctx) =>
                {
                    CheckLink(ctx, nameof(links.Whatsapp), ContactPlatform.Whatsapp, links.Whatsapp);
                    CheckLink(ctx, nameof(links.Facebook), ContactPlatform.Facebook, links.Facebook);
                    CheckLink(ctx, nameof(links.Instagram), ContactPlatform.Instagram, links.Instagram);
                    CheckLink(ctx, nameof(links.Youtube), ContactPlatform.Youtube, links.Youtube);
                    CheckLink(ctx, nameof(links.Telegram), ContactPlatform.Telegram, links.Telegram);
                    CheckLink(ctx, nameof(links.Tiktok), ContactPlatform.Tiktok, links.Tiktok);
                    CheckLink(ctx, nameof(links.Website), ContactPlatform.Website, links.Website);
                }
            );
    }

    private static void NamePart(IRuleBuilderInitial<UpdateMyProfileCommand, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .Must(v => !string.IsNullOrWhiteSpace(v))
            .WithMessage("VALIDATION.NAME_PARTS_REQUIRED")
            .Must(v => v.Trim().Length >= 2)
            .WithMessage("VALIDATION.NAME_PARTS_REQUIRED")
            .Must(v => v.Trim().Length <= 20)
            .WithMessage("VALIDATION.NAME_PART_MAX_LENGTH")
            .Must(v => NamePattern.IsMatch(v.Trim()))
            .WithMessage("VALIDATION.NAME_LETTERS_ONLY");

    // The identifier is "Links.Facebook"; the front maps the last segment to links.facebook.
    private static void CheckLink(
        ValidationContext<UpdateMyProfileCommand> ctx,
        string field,
        ContactPlatform platform,
        string? value
    )
    {
        var result = ContactLinkNormalizer.Normalize(platform, value);
        if (!result.IsValid)
        {
            ctx.AddFailure($"Links.{field}", result.ErrorKey!);
        }
    }
}