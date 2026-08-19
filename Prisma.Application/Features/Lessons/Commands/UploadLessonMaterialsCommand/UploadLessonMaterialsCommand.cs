using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Prisma.Application.Features.Lessons.Commands.UploadLessonMaterialsCommand;

public record UploadLessonMaterialsCommand(int LessonId, List<IFormFile> Files) : IRequest<Result>;
