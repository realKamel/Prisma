using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;

namespace Prisma.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(request.Id, cancellationToken);
        if (user is null)
            throw new NotFoundException(nameof(User), request.Id);

        var result = await identityService.DeleteAsync(user);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join("\n", result.Errors.Select(e => e.Description)));

        return Result.Success("User deleted successfully.");
    }
}