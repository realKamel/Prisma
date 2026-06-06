namespace Prisma.Application.Abstractions.Identity;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}