namespace Prisma.Application.Common.Time;

internal static class CairoDate
{
    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    public static DateOnly Of(DateTimeOffset instant) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Cairo).DateTime);

    public static DateOnly Today() => Of(DateTimeOffset.UtcNow);
}