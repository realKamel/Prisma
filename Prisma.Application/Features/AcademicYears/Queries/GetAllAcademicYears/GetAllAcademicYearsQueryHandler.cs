using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;

internal class GetAllAcademicYearsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAcademicYearsQuery, Result<List<AcademicYearOptionDto>>>
{
    public async Task<Result<List<AcademicYearOptionDto>>> Handle(GetAllAcademicYearsQuery request, CancellationToken ct)
    {
        var repo = unitOfWork.GetOrCreateRepository<AcademicYear, int>();
        var years = await repo.ListAsync(new AllAcademicYearsSpecification(), ct);

        return years.Select(a => new AcademicYearOptionDto
        {
            AcademicYearId = a.Id,
            Name = a.Title ?? string.Empty
        }).ToList();
    }
}
