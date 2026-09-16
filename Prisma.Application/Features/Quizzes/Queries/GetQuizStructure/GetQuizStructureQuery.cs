using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizStructure;

public record GetQuizStructureQuery(int QuizId) : IRequest<Result<QuizStructureDto>>;
