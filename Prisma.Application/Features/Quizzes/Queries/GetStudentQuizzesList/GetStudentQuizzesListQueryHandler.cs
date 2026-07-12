using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetStudentQuizzesList;

public class GetStudentQuizzesListQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : IRequestHandler<GetStudentQuizzesListQuery, Result<StudentQuizzesListResponseDto>>
{

    public async Task<Result<StudentQuizzesListResponseDto>> Handle(GetStudentQuizzesListQuery request, CancellationToken ct)
    {
        var studentId = currentUser.UserId!.Value;

        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var academicYearId = await studentRepo.FirstOrDefaultAsync(new StudentWithAcademicYearSpecification(studentId), ct);

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var enrolledLessonIds = await enrollmentRepo.ListAsync(new EnrolledLessonIdsSpecification(studentId), ct);

        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();

        var quizzes = await quizRepo.ListAsync(
    new StudentQuizzesSpecification(enrolledLessonIds, academicYearId, studentId), ct);

        var now = DateTimeOffset.UtcNow;

        var raw = 
            quizzes.Select(q => new
            {
                q.Id,
                q.Title,
                q.TotalDegree,
                q.AvailableFrom,
                q.DueDate,
                DurationMinutes = (int)q.TimeInMinutes.TotalMinutes,
                QuestionsCount = q.Questions.Count,
                Attempt = q.Attempts
                    .Select(a => new { a.Id, a.Status, a.SubmittedAt, a.Degree })
                    .FirstOrDefault()
                
            });

        var items = raw.Select(q =>
        {
            string status;
            decimal? score = null;
            DateTimeOffset? submittedAt = null;
            var dueDatePassed = !q.DueDate.HasValue || now >= q.DueDate.Value;


            if (q.Attempt is null)
            {
                if (q.AvailableFrom.HasValue && q.AvailableFrom.Value > now)
                    status = "upcoming";
                else if (q.DueDate.HasValue && q.DueDate.Value < now)
                    status = "missed";
                else
                    status = "new";
            }
            else if (q.Attempt.Status == QuizAttemptStatus.Graded && dueDatePassed)
            {
                status = "done";
                score = q.Attempt.Degree;
                submittedAt = q.Attempt.SubmittedAt;
            }
            else
            {
                status = "pending";
                submittedAt = q.Attempt.SubmittedAt;
            }

            return new StudentQuizListItemDto
            {
                QuizId = q.Id,
                Title = q.Title ?? string.Empty,
                Status = status,
                AvailableFrom =  q.AvailableFrom ,
                DueDate = q.DueDate,
                SubmittedAt = submittedAt,
                Score = score,
                TotalDegree = q.TotalDegree,
                QuestionsCount = q.QuestionsCount,
                DurationMinutes = q.DurationMinutes
            };
        }).ToList();


        var doneScores = items.Where(i => i.Status == "done" && i.Score.HasValue && i.TotalDegree > 0)
            .Select(i => (double)(i.Score!.Value / i.TotalDegree * 100))
            .ToList();

        var stats = new StudentQuizzesStatsDto
        {
            Total = items.Count,
            NewCount = items.Count(i => i.Status == "new"),
            PendingCount = items.Count(i => i.Status == "pending"),
            DoneCount = items.Count(i => i.Status == "done"),
            MissedCount = items.Count(i => i.Status == "missed"),
            UpcomingCount = items.Count(i => i.Status == "upcoming"),
            AverageScorePercent = doneScores.Count > 0 ? Math.Round(doneScores.Average(), 1) : 0,
            BestScorePercent = doneScores.Count > 0 ? Math.Round(doneScores.Max(), 1) : 0
        };

        if (!string.IsNullOrWhiteSpace(request.Filter) && request.Filter != "all")
            items = items.Where(i => i.Status == request.Filter).ToList();

        return Result<StudentQuizzesListResponseDto>.Success(new StudentQuizzesListResponseDto
        {
            Stats = stats,
            Items = items
        });
    }


    
}
