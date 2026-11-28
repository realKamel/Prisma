using FluentValidation;

namespace Prisma.Application.Features.Users.Commands.MarkTourSeen;

public sealed class MarkTourSeenCommandValidator : AbstractValidator<MarkTourSeenCommand>
{
    public MarkTourSeenCommandValidator()
    {
        RuleFor(x => x.TourKey).NotEmpty().MaximumLength(64);
    }
}