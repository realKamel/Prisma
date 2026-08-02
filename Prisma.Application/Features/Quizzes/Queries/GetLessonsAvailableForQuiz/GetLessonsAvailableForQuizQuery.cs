using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;

public record GetLessonsAvailableForQuizQuery : IRequest<Result<List<LessonOptionDto>>>;
