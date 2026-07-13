using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Common.DTOs.Ai;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.QuizAttemptSpecs;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Infrastructure.AgenticWorkflows.ReportGeneratorWorkflow;

public partial class GetStudentStatusExecutor(IServiceScopeFactory serviceProvider)
    : Executor("FetchStudentStatusExecutor")
{
    [MessageHandler]
    private async ValueTask<StudentData> HandleAsync(Guid message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        // var now = DateTimeOffset.UtcNow;
        //
        // using var scope = serviceProvider.CreateScope();
        //
        // var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        //
        // var enrollmentRepo = uow.GetOrCreateRepository<Enrollment, int>();
        // var studentRepo = uow.GetOrCreateRepository<Student, Guid>();
        // var attemptsRepo = uow.GetOrCreateRepository<QuizAttempt, int>();
        //
        // var studentResult =
        //     await studentRepo.FirstOrDefaultAsync(
        //         new StudentWithProjectionSpec<StudentNames>(
        //             message,
        //             s =>
        //                 new StudentNames(s.FirstName, s.LastName)),
        //         cancellationToken);
        //
        // var enrollments = await enrollmentRepo
        //     .ListAsync(
        //         new EnrollmentAndLessonAndLessonProgressesOrderByCreatedAtDesc
        //             <EnrollmentReportDto>(e =>
        //                     e.StudentId == message && e.CreatedAt > now.AddDays(-7),
        //                 e => new EnrollmentReportDto(e.Id, e.IsCompleted,
        //                     new LessonReportDto(e.Lesson.Title ?? "", e.Lesson.Assignment.Grade))),
        //         cancellationToken);
        //
        // var attempts = await attemptsRepo
        //     .ListAsync(new StudentAttemptsSpec<AttemptReportDto>(message, x =>
        //             new AttemptReportDto(x.Quiz.Title ?? x.QuizId.ToString(), x.Degree)),
        //         cancellationToken);
        //
        return
            new StudentData(
                message,
                new("", ""),
                [],
                []
            );
    }
}