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
}
