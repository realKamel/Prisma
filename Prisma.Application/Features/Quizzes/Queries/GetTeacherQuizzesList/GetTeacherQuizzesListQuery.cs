using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizzesList;

public record GetTeacherQuizzesListQuery(
    QuizScope Scope,
    string? Search,
    string? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<TeacherQuizzesListResponseDto>>;
