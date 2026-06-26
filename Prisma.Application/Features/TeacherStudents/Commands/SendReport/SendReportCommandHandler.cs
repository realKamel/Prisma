using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Commands.SendReport;

public class SendReportCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SendReportCommand, Result>
{
    public async Task<Result> Handle(SendReportCommand request, CancellationToken cancellationToken)
    {
        var studentRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.UserAggregate.Student, Guid>();
        var reportRepo = unitOfWork.GetOrCreateRepository<Report, int>();

        foreach (var studentId in request.StudentIds)
        {
            var student = await studentRepo.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
                throw new NotFoundException("Student", studentId);

            var report = new Report
            {
                StudentId = studentId,
                Content = $"Report Type: {request.ReportType} | Period: {request.DateFrom} to {request.DateTo} | Sent via WhatsApp",
                Date = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await reportRepo.AddAsync(report, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success($"{request.StudentIds.Count} report(s) queued for sending via WhatsApp.");
    }
}