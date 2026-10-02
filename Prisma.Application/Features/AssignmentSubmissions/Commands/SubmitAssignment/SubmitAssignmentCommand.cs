using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Prisma.Application.Features.AssignmentSubmissions.Commands.SubmitAssignment;

public record SubmitAssignmentCommand(int LessonId, IFormFile File) : IRequest<Result>;