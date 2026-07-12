using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizResult;

public record GetQuizResultQuery(int QuizId) : IRequest<Result<QuizResultDto>>;

