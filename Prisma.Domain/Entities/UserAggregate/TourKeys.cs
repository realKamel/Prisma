using System.Collections.Frozen;

namespace Prisma.Domain.Entities.UserAggregate;

/// <summary>Every tour the app can show, grouped by who takes it. A new tour needs one line here and one on the frontend.</summary>
public static class TourKeys
{
    public static class ForStudent
    {
        public const string Welcome = "student-welcome";
        public const string Teachers = "student-teachers";
        public const string Lessons = "student-lessons";
        public const string Purchase = "student-purchase";
        public const string Player = "student-player";
        public const string Quiz = "student-quiz";
        public const string History = "student-history";

        public static readonly FrozenSet<string> All = new[]
        {
            Welcome, Teachers, Lessons, Purchase, Player, Quiz, History,
        }.ToFrozenSet();
    }

    public static class ForTeacher
    {
        public const string Welcome = "teacher-welcome";

        public static readonly FrozenSet<string> All = new[] { Welcome }.ToFrozenSet();
    }

    public static class ForAssistant
    {
        public const string Welcome = "assistant-welcome";

        public static readonly FrozenSet<string> All = new[] { Welcome }.ToFrozenSet();
    }
}