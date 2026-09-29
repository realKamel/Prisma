namespace Prisma.Domain.ReadModels.Enrollments;

public sealed record StudentPerformanceReadModel(
    int TotalLessons,
    int CompletedLessons,
    int TotalStudyHours,
    decimal AverageQuizDegree
);