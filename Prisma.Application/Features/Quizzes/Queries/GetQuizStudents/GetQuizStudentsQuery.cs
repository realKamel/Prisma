using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizStudents;

public record GetQuizStudentsQuery(
    int QuizId,
    string? Search,   // search by student name
    string? Status,   // filter by attempt status
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<QuizStudentsResponseDto>>;