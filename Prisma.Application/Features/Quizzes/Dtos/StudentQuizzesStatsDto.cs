namespace Prisma.Application.Features.Quizzes.Dtos;

public class StudentQuizzesStatsDto
{
    public int Total { get; set; }
    public double AverageScorePercent { get; set; }
    public double BestScorePercent { get; set; }
    public int NewCount { get; set; }
    public int PendingCount { get; set; }
    public int DoneCount { get; set; }
    public int MissedCount { get; set; }
    public int UpcomingCount { get; set; }

}
