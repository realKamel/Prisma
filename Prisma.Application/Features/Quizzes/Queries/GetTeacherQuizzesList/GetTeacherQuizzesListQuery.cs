using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizzesList;

public record GetTeacherQuizzesListQuery(
    QuizScope Scope,
    string? Search,
    string? Status
) : IRequest<Result<List<TeacherQuizListItemDto>>>;
