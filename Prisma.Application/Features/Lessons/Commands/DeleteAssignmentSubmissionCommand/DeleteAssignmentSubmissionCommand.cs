using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Lessons.Commands.DeleteAssignmentSubmissionCommand;

public record DeleteSubmissionCommand(int LessonId) : IRequest<Result<string>>;