using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Prisma.Application.Features.Lessons.Commands.SubmitAssignmentCommand;

public record SubmitAssignmentCommand(int LessonId, IFormFile File) : IRequest<Result>;
