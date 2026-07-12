namespace Prisma.Infrastructure.AgenticWorkflows.DTOs;

public record EnrollmentReportDto(int EnrollmentId, bool IsCompleted, LessonReportDto LessonReport);