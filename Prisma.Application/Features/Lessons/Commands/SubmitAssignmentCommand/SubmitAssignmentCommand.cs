using MediatR;
using Microsoft.AspNetCore.Http;
using Ardalis.Result;

namespace Prisma.Application.Features.Lessons.Commands.SubmitAssignmentCommand;

public record SubmitAssignmentCommand(
    int LessonId,
    IFormFile File
) : IRequest<Result<string>>;

