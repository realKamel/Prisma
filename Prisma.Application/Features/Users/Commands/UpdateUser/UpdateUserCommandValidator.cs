using FluentValidation;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.FirstName).MustBeValidPersonName();
        RuleFor(x => x.SecondName).MustBeValidPersonName();
        RuleFor(x => x.ThirdName).MustBeValidPersonName();
        RuleFor(x => x.LastName).MustBeValidPersonName();
        RuleFor(x => x.Mobile).MustBeValidPhoneNumber();

        RuleFor(x => x.Email).MustBeValidEmail();

        RuleFor(x => x.NewPassword)
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[^a-zA-Z0-9]")
            .When(x => !string.IsNullOrWhiteSpace(x.NewPassword));

        RuleFor(x => x.ParentMobile)
            .MustBeValidPhoneNumber()
            .When(x => !string.IsNullOrEmpty(x.ParentMobile));
    }
}
