using MediatR;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Commands.SubmitAssignmentCommand;

public record SubmitAssignmentCommand(
    int LessonId,
    IFormFile File
) : IRequest<Result<string>>;

