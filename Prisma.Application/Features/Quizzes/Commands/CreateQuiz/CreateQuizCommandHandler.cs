using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Quizzes.Commands.CreateQuiz;

public class CreateQuizCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IRequestHandler<CreateQuizCommand, Result<TeacherQuizListItemDto>>
{
    public async Task<Result<TeacherQuizListItemDto>> Handle(CreateQuizCommand request, CancellationToken ct)
    {
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        Lesson? lesson = null;

        // if scope = LessonQuiz => Make sure lesson exsits and doesn't contain a quiz
        if (request.Scope == QuizScope.LessonQuiz)
        {
             lesson = await lessonRepo.FirstOrDefaultAsync(new LessonByIdSpecification( request.LessonId!.Value), ct);

            if (lesson is null)
                return Result<TeacherQuizListItemDto>.Failure("الحصة غير موجودة");

            if (lesson.QuizId.HasValue)
                return Result<TeacherQuizListItemDto>.Failure("الحصة دي عندها اختبار بالفعل");
        }

        // بناء الأسئلة + الاختيارات
        var questions = new List<Question>();
        decimal totalDegree = 0;


        var questionRepo = unitOfWork.GetOrCreateRepository<Question, int>();

        foreach(var qDto in request.Questions)
        {
            Question question = qDto.Type == QuestionType.Written
                ? new WrittenQuestion { Title = qDto.Text, Type = qDto.Type, Answer = qDto.ModelAnswer }
                : new MCQQuestion
                {
                    Title = qDto.Text,
                    Type = qDto.Type,
                    Choices = qDto.Choices!.Select(c => new Choice
                    {
                        Text = c.Text,
                        IsCorrect = c.IsCorrect
                    }).ToList()
                };

            questionRepo.Add(question);
            questions.Add(question);
            totalDegree += qDto.Degree;
        }
        await unitOfWork.SaveChangesAsync(ct);

        var quiz = new Quiz
        {
            Title = request.Title,
            Description = request.Description,
            Scope = request.Scope,
            LessonId = request.Scope == QuizScope.LessonQuiz ? request.LessonId : null,
            AcademicYearId = request.Scope == QuizScope.ComprehensiveExam ? request.AcademicYearId : null,
            TimeInMinutes = TimeSpan.FromMinutes(request.DurationMinutes),
            AvailableFrom = request.AvailableFrom,
            DueDate = request.DueDate,
            TotalDegree = totalDegree
        };

        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();
        quizRepo.Add(quiz);
        await unitOfWork.SaveChangesAsync(ct);


        var questionLinks = new List<QuestionLessonQuiz>();
        for (int i = 0; i < questions.Count; i++)
        {
            questionLinks.Add(new QuestionLessonQuiz
            {
                LessonQuizId = quiz.Id,
                QuestionId = questions[i].Id,
                Degree = request.Questions[i].Degree
            });
        }

        var linkRepo = unitOfWork.GetOrCreateRepository<QuestionLessonQuiz, int>();
        linkRepo.AddRange(questionLinks);

        // ربط الـ Lesson بالـ Quiz (one-to-one) لو Scope == LessonQuiz
        if (request.Scope == QuizScope.LessonQuiz)
        {
            lesson!.QuizId = quiz.Id;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result<TeacherQuizListItemDto>.Success(new TeacherQuizListItemDto
        {
            QuizId = quiz.Id,
            Title = quiz.Title ?? string.Empty,
            Description = quiz.Description,
            DurationMinutes = (int)quiz.TimeInMinutes.TotalMinutes,
            QuestionsCount = request.Questions.Count,
            TotalDegree = totalDegree,
            AvailableFrom = quiz.AvailableFrom,
            DueDate = quiz.DueDate,

            // New quiz has no attempts yet
            SubmittedCount = 0,
            PendingGradingCount = 0,
            AverageScore = null,
            Status = "active"
        }, "تم إنشاء الاختبار بنجاح");
   
    }
}
