using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentStats;

public class GetStudentStatsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetStudentStatsQuery, StudentStatsDto>
{
    public async Task<StudentStatsDto> Handle(GetStudentStatsQuery request, CancellationToken cancellationToken)
    {
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.EnrollmentAggregate.Enrollment, int>();
        var quizAttemptRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.QuizAggregate.QuizAttempt, int>();
        var sectionProgressRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.LessonAggregate.SectionProgress, int>();
        var assignmentSubmissionRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.LessonAggregate.AssignmentSubmission, int>();

        var enrollments = await enrollmentRepo.ListAsync(
            new StatsEnrollmentsByStudentSpec(request.StudentId), cancellationToken);
        var quizAttempts = await quizAttemptRepo.ListAsync(
            new StatsQuizAttemptsByStudentSpec(request.StudentId), cancellationToken);
        var sectionProgresses = await sectionProgressRepo.ListAsync(
            new StatsSectionProgressByStudentSpec(request.StudentId), cancellationToken);
        var submissions = await assignmentSubmissionRepo.ListAsync(
            new StatsAssignmentSubmissionsByStudentSpec(request.StudentId), cancellationToken);

        var lessons = enrollments.Count;
        var avgQuiz = quizAttempts.Any() ? (int)quizAttempts.Average(q => q.Degree) : 0;

        // Sum of completion percentages as proxy for "hours" (0-100 scale per section)
        var hours = sectionProgresses.Sum(sp => sp.Percentage) / 100;

        // Submissions that don't have a FileUrl yet = pending
        var pending = submissions.Count(s => string.IsNullOrEmpty(s.FileUrl));

        return new StudentStatsDto(lessons, avgQuiz, hours, pending);
    }
}