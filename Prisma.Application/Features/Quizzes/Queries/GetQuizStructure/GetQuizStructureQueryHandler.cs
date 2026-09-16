using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizStructure;

public class GetQuizStructureQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetQuizStructureQuery, Result<QuizStructureDto>>
{
    public async Task<Result<QuizStructureDto>> Handle(GetQuizStructureQuery request, CancellationToken ct)
    {
        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();
        var quiz = await quizRepo.FirstOrDefaultAsync(new QuizStructureSpecification(request.QuizId), ct);

        if (quiz is null)
            return Result<QuizStructureDto>.Error("الاختبار غير موجود");

        var dto = new QuizStructureDto
        {
            QuizId = quiz.Id,
            Title = quiz.Title ?? string.Empty,
            TeacherName = quiz.Lesson?.Teacher is not null
                ? $"{quiz.Lesson.Teacher.FirstName} {quiz.Lesson.Teacher.LastName}"
                : string.Empty,
            Subject = quiz.Lesson?.Teacher?.Subject ?? string.Empty,
            Instructions = quiz.Description ?? "لا يوجد تعليمات لهذا الإختبار",
            DurationMinutes = (int)quiz.TimeInMinutes.TotalMinutes,
            Questions = quiz.Questions.Select(ql =>
            {
                var q = ql.Question;
                List<QuizChoiceDto>? choices = null;
                if (q is MCQQuestion mcq)
                {
                    choices = mcq.Choices.Select(c => new QuizChoiceDto
                    {
                        ChoiceId = c.Id,
                        Text = c.Text ?? string.Empty
                    }).ToList();
                }

                return new QuizQuestionStructureDto
                {
                    QuestionId = q.Id,
                    Text = q.Title,
                    Type = q.Type,
                    Degree = ql.Degree,
                    Choices = choices
                };
            }).ToList()
        };

        return Result<QuizStructureDto>.Success(dto);
    }
}
