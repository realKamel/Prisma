using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RedeemCodes.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetCodeLessonOptions;

internal class GetCodeLessonOptionsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<GetCodeLessonOptionsQuery, Result<List<CodeLessonOptionDto>>>
{
    public async Task<Result<List<CodeLessonOptionDto>>> Handle(
        GetCodeLessonOptionsQuery request,
        CancellationToken ct)
    {
        if (currentUser.UserId is not { } teacherId)
            throw new UnauthorizedException("User is not authenticated.");

        var repo = unitOfWork.GetOrCreateRepository<AcademicYearLesson, int>();
        var links = await repo.ListAsync(
            new TeacherAcademicYearLessonsSpecification(teacherId), ct);

        var result = links.Select(x => new CodeLessonOptionDto
        {
            Id = x.LessonId,
            Name = x.Lesson.Title ?? string.Empty,
            AcademicYearId = x.AcademicYearId,
        }).ToList();

        return result;
    }
}