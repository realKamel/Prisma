namespace Prisma.Domain.ReadModels.Enrollments;

public record StudentPerformanceReadModel(
    int TotalLessons,
    int CompletedLessons,
    int TotalStudyHours,
    decimal AverageQuizDegree
);
