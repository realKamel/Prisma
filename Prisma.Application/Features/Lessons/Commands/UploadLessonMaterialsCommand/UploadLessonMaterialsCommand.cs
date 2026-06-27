using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Commands.UploadLessonMaterials;

public record UploadLessonMaterialsCommand(
    int LessonId,
    List<IFormFile> Files
) : IRequest<Result<string>>;