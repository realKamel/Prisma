using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;

namespace Prisma.Application.Features.Assistants.Commands.DeleteAssistant;

public class DeleteAssistantCommandHandler(IIdentityService identityService)
    : IRequestHandler<DeleteAssistantCommand, Result>
{
    public async Task<Result> Handle(DeleteAssistantCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(request.AssistantId, cancellationToken);

        if (user is null)
        {
            return Result.NotFound($"Assistant with id '{request.AssistantId}' was not found");
        }

        var result = await identityService.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return Result.Error(string.Join("\n", result.Errors.Select(e => e.Description)));
        }

        return Result.Success();
    }
}