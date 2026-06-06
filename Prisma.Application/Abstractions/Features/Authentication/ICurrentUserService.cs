namespace Prisma.Application.Abstractions.Features.Authentication;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}