using FluentValidation;

namespace Prisma.Application.Features.Assistants.Commands.DeleteAssistant;

public class DeleteAssistantCommandValidator : AbstractValidator<DeleteAssistantCommand>
{
    public DeleteAssistantCommandValidator()
    {
        RuleFor(e => e.AssistantId)
            .NotEmpty()
            .WithMessage("The assistant id is required.");
    }
}