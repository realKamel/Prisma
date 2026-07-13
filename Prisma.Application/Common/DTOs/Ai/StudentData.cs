namespace Prisma.Application.Common.DTOs.Ai;

public record StudentData(
    Guid StudentId,
    StudentNames StudentName,
    IEnumerable<EnrollmentReportDto> Enrollments,
    IEnumerable<AttemptReportDto> Attempts);