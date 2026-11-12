namespace Prisma.Application.Features.Quizzes.Common;

public static class QuizCacheKeys
{
    // NOTE: any command that changes quiz content (title, time, questions, choices) must
    // invalidate Structure(quizId) after SaveChanges, or students see stale data until the TTL.

    public static string Structure(int quizId) => $"quiz-structure:v1:{quizId}";
    public static string QuizTag(int quizId) => $"quiz:{quizId}";
}