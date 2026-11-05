namespace Prisma.Application.Common.Extenions;

public static class IntExtension
{
    public static long DayInSeconds(this int number)
    {
        return number * 86400;
    }

    public static long DayInMinutes(this int number)
    {
        return number * 1440;
    }

    public static long DayInHours(this int number)
    {
        return number * 24;
    }

    public static TimeSpan Days(this int number) => TimeSpan.FromDays(number);

    public static TimeSpan Hours(this int number) => TimeSpan.FromHours(number);

    public static TimeSpan Minutes(this int number) => TimeSpan.FromMinutes(number);
}
