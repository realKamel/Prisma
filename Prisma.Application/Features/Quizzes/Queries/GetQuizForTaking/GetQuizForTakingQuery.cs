using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizForTaking;

public record GetQuizForTakingQuery(int QuizId) : IRequest<Result<QuizTakingDto>>;
