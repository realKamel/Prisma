using FluentValidation;
using Prisma.Application.Common.Validators;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).SetValidator(new PersonNameValidator());
        RuleFor(x => x.SecondName).SetValidator(new PersonNameValidator());
        RuleFor(x => x.ThirdName).SetValidator(new PersonNameValidator());
        RuleFor(x => x.LastName).SetValidator(new PersonNameValidator());
        RuleFor(x => x.Mobile).EgyptianPhoneNumber();
        RuleFor(x => x.Email).Email();

        RuleFor(x => x.NewPassword)
            .MinimumLength(8).MaximumLength(128)
            .Matches("[A-Z]").Matches("[a-z]").Matches("[0-9]").Matches("[^a-zA-Z0-9]")
            .When(x => !string.IsNullOrWhiteSpace(x.NewPassword));

        RuleFor(x => x.ParentMobile)
            .EgyptianPhoneNumber()
            .When(x => !string.IsNullOrEmpty(x.ParentMobile));
    }
}