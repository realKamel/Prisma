using MediatR;
using Microsoft.Extensions.Logging;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Common.DTOs.Ai;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.QuizAttemptSpecs;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Features.Reports.Commands.GenerateWeeklyReport;

internal class GenerateWeeklyReportCommandHandler(
    IUnitOfWork uow,
    IReportGenerator reportGenerator,
    ILogger<GenerateWeeklyReportCommandHandler> logger)
    : IRequestHandler<GenerateWeeklyReportCommand>
{
    public async Task Handle(GenerateWeeklyReportCommand request, CancellationToken cancellationToken)
    {
        var enrollmentRepo = uow.GetOrCreateRepository<Enrollment, int>();
        var studentRepo = uow.GetOrCreateRepository<Student, Guid>();
        var attemptsRepo = uow.GetOrCreateRepository<QuizAttempt, int>();

        var now = DateTimeOffset.UtcNow;
        var weekStart = now.AddDays(-7);

        var students = await studentRepo.ListAsync(new ActiveStudentIds(), cancellationToken);

        var studentBatches = students.Chunk(10);

        foreach (var batch in studentBatches)
        {
            foreach (var studentId in batch)
            {
                try
                {
                    // Idempotency guard: skip if this student already has a report
                    // for this week (covers retries after partial batch failure).
                    var alreadyReported = await studentRepo.AnyAsync(
                        new StudentHasReportSinceSpec(studentId, weekStart),
                        cancellationToken);

                    if (alreadyReported)
                    {
                        logger.LogInformation(
                            "Skipping weekly report for student {StudentId}, already generated this week",
                            studentId);
                        continue;
                    }

                    var student = await studentRepo.FirstOrDefaultAsync(
                        new StudentWithReportsSpec(studentId),
                        // include Reports nav if needed
                        cancellationToken);

                    if (student is null)
                    {
                        logger.LogWarning(
                            "Active student {StudentId} not found during weekly report generation",
                            studentId);
                        continue;
                    }

                    var studentNames = new StudentNames(student.FirstName, student.LastName);

                    var enrollments = await enrollmentRepo.ListAsync(
                        new EnrollmentAndLessonAndLessonProgressesOrderByCreatedAtDesc<EnrollmentReportDto>(
                            e => e.StudentId == studentId && e.CreatedAt > weekStart,
                            e => new EnrollmentReportDto(e.Id, e.IsCompleted,
                                new LessonReportDto(e.Lesson.Title ?? "", e.Lesson.Assignment.Grade))),
                        cancellationToken);

                    var attempts = await attemptsRepo.ListAsync(
                        new StudentAttemptsSpec<AttemptReportDto>(studentId,
                            x => new AttemptReportDto(x.Quiz.Title ?? x.QuizId.ToString(), x.Degree)),
                        cancellationToken);

                    var report = await reportGenerator.GenerateReportAsync(
                        new StudentData(studentId, studentNames, enrollments, attempts),
                        cancellationToken);

                    student.Reports.Add(new Report
                    {
                        StudentId = studentId,
                        Content = report,
                        Date = now,
                    });
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // one student's AI/DB fail can kill the whole run.
                    logger.LogError(ex,
                        "Failed to generate weekly report for student {StudentId}", studentId);
                }
            }

            await uow.SaveChangesAsync(cancellationToken);
        }
    }
}