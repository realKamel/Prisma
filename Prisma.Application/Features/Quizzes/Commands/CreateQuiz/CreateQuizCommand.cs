
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Commands.CreateQuiz;

public record CreateQuizCommand(
    string Title,
    string? Description,
    QuizScope Scope,
    int? LessonId,              // مطلوب لو Scope == LessonQuiz
    int? AcademicYearId,        // مطلوب لو Scope == ComprehensiveExam
    int DurationMinutes,
    DateTimeOffset? AvailableFrom,
    DateTimeOffset? DueDate,
    List<CreateQuizQuestionDto> Questions
) : IRequest<Result<TeacherQuizListItemDto>>;