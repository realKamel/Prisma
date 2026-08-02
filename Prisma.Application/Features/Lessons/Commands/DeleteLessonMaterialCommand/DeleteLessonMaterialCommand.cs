using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterialCommand;


public record DeleteLessonMaterialCommand(
    int LessonId 
, int MaterialId) : IRequest<Result<string>>;

