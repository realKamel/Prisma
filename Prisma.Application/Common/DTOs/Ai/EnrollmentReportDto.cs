namespace Prisma.Application.Common.DTOs.Ai;

public record EnrollmentReportDto(int EnrollmentId, bool IsCompleted, LessonReportDto LessonReport);