using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.TeacherStudents.Commands.SendReport;

public record SendReportCommand(
    List<Guid> StudentIds,
    string ReportType,
    string DateFrom,
    string DateTo) : IRequest<Result>;
