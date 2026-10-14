using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.AssignmentSubmissions.Commands.DeleteAssignmentSubmission;

public record DeleteAssignmentSubmissionCommand(Guid LessonId) : IRequest<Result>;
