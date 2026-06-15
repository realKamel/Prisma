using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonStatus;

public record GetLessonStatusQuery(int id)  : IRequest<Result<LessonStatusResponse>>;

public class LessonStatusResponse
{
    public LessonCatalogStatus Status { get; set; }
}