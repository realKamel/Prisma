using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterialCommand;


public record DeleteLessonMaterialCommand(
    int LessonId 
, int MaterialId) : IRequest<Result<string>>;

