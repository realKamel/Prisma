using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterialCommand;

public record DeleteLessonMaterialCommand(int LessonId, int MaterialId) : IRequest<Result>;
