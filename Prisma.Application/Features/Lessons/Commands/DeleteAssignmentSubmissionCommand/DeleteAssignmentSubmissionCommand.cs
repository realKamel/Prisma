using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Lessons.Commands.DeleteAssignmentSubmissionCommand;

public record DeleteSubmissionCommand(int LessonId) : IRequest<Result>;
