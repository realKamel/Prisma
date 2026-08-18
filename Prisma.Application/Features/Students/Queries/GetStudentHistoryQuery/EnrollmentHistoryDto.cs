namespace Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;

public record EnrollmentHistoryDto(
    Guid PublicId,
    string ImageThumbnailUrl,
    string Title,
    string Status,
    string TeacherName,
    string Subject,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? ExpiresAt,
    decimal TotalDegree,
    bool IsCompleted,
    int SectionsCount,
    double TotalProgress
);
