using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Commands.DeleteAssignmentSubmissionCommand;
public record DeleteSubmissionCommand(int LessonId) : IRequest<Result<string>>;