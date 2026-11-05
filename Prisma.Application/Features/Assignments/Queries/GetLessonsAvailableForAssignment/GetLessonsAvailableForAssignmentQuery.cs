using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Assignments.Queries.GetLessonsAvailableForAssignment;

public record GetLessonsAvailableForAssignmentQuery : IRequest<Result<List<LessonOptionDto>>>;
