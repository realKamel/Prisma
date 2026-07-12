namespace Prisma.API.Features.TeacherStudents.Requests;

public record SendReportRequest(
    List<Guid> StudentIds,
    string ReportType,
    string DateFrom,
    string DateTo);
