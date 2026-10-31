using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.TeacherStudents.Commands.SendReport;

public sealed record SendReportCommand(
    IList<Guid> StudentIds,
    string ReportType,
    string DateFrom,
    string DateTo
) : IRequest<Result>;
