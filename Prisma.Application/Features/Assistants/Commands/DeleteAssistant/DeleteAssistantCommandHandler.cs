using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;

namespace Prisma.Application.Features.Assistants.Commands.DeleteAssistant;

public class DeleteAssistantCommandHandler(IIdentityService identityService)
    : IRequestHandler<DeleteAssistantCommand, Result>
{
    public async Task<Result> Handle(DeleteAssistantCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(request.AssistantId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(Assistant), request.ToString());
        }

        var result = await identityService.DeleteAsync(user);

        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("\n", result.Errors.Select(e => e.Description)));
        }

        return Result.Success();
    }
}