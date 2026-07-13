using Prisma.Application.Common.DTOs.Ai;

namespace Prisma.Application.Abstractions.Ai;

public interface IReportGenerator
{
    Task<string> GenerateReportAsync(StudentData request, CancellationToken ct);
}

public enum ReportType
{
    TeacherPerformanceSummary,
    StudentProgressReport,
    LessonCompletionReport,
    WeeklyEarningsSummary
}

public sealed record ReportRequest(
    string StudentName,
    List<AttemptReportDto> Attempts,
    List<EnrollmentReportDto> Enrollments,
    string? AdditionalContext = null);

public sealed record ReportMetric(string Label, string Value, string? Trend = null);