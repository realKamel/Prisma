namespace Prisma.Infrastructure.AgenticWorkflows.DTOs;

public record StudentData(
    Guid StudentId,
    string StudentName,
    IEnumerable<EnrollmentReportDto> Enrollments,
    IEnumerable<AttemptReportDto> Attempts);