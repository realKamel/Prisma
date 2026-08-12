using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.BackgroundJobs;

namespace Prisma.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler(IBackgroundJobService backgroundJobService)
    : IRequestHandler<LogoutCommand, Result>
{
    public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        backgroundJobService
            .Enqueue<ILogoutUserJob>(j => j.Clean(request.AccessToken));

        return Task.FromResult(Result.Success());
    }
}