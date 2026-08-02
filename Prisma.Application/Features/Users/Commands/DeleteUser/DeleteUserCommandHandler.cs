using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.NotFound($"User with id '{request.Id}' was not found");
        }


        var result = await identityService.DeleteAsync(user);

        if (!result.Succeeded)
            return Result.Error(string.Join("\n", result.Errors.Select(e => e.Description)));

        return Result.NoContent();
    }
}