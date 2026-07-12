using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Application.Abstractions.Ai;

public interface IReportGenerator
{
    Task<string> GenerateReportAsync(ReportRequest request, CancellationToken ct);
}

public enum ReportType
{
    TeacherPerformanceSummary,
    StudentProgressReport,
    CourseCompletionReport,
    WeeklyEarningsSummary
}

public sealed record ReportRequest(
    ReportType Type,
    string RecipientName,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    IReadOnlyList<ReportMetric> Metrics,
    string? AdditionalContext = null);

public sealed record ReportMetric(string Label, string Value, string? Trend = null);