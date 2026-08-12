using MediatR;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Features.Authentication.Commands.CleanUserAuthTokens;

namespace Prisma.Infrastructure.BackgroundJobs.Jobs;

public class CleanUpAuth(ISender sender) : ILogoutUserJob
{
    public async Task Clean(string? accessToken)
    {
        await sender.Send(new CleanUserAuthTokensCommand(accessToken));
    }
}