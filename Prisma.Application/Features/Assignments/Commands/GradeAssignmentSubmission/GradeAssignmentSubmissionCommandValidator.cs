using FluentValidation;

namespace Prisma.Application.Features.Assignments.Commands.GradeAssignmentSubmission;

public class GradeAssignmentSubmissionCommandValidator
    : AbstractValidator<GradeAssignmentSubmissionCommand>
{
    public GradeAssignmentSubmissionCommandValidator()
    {
        RuleFor(x => x.SubmissionId).GreaterThan(0);

        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0).WithMessage("الدرجة لازم تكون صفر أو أكبر");

        RuleFor(x => x.Note)
            .MaximumLength(1000).When(x => x.Note is not null);
    }
}
