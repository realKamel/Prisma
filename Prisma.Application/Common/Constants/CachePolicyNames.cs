namespace Prisma.Application.Common.Constants;

public static class CachePolicyNames
{
    public static class Short
    {
        public const string Name = "ShortCache";
        public const int DurationSeconds = 20;
        public static TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);
    }

    public static class Long
    {
        public const string Name = "LongCache";
        public const int DurationMinutes = 5;
        public static TimeSpan Duration => TimeSpan.FromMinutes(DurationMinutes);
    }
}