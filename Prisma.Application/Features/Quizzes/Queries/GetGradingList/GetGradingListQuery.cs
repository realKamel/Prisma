using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Queries.GetGradingList;

public record GetGradingListQuery(
    QuizScope Scope,
    string? Search,
    string? Status,
    int? QuizId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<GradingListResponseDto>>;
