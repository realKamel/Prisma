using Ardalis.Result;
using MediatR;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonStatus;

public record GetLessonStatusQuery(int LessonId) : IRequest<Result<LessonStatusResponse>>;

public record LessonStatusResponse(EnrollmentStatus Status);
