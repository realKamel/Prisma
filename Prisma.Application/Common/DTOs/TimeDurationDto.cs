using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Common.DTOs;

public sealed record TimeDurationDto(long Seconds = 0);

public static class TimeDurationExtensions
{
    public static TimeDurationDto ToDto(this TimeDuration entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new TimeDurationDto(entity.Seconds);
    }
}
