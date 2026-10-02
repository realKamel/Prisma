using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.AssignmentSubmissions.Commands.DeleteAssignmentSubmission;

public record DeleteAssignmentSubmissionCommand(int LessonId) : IRequest<Result>;
