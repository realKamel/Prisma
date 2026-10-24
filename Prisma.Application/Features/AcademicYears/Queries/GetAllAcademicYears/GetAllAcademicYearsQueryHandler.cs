using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;

internal sealed class GetAllAcademicYearsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAcademicYearsQuery, Result<List<AcademicYearOptionDto>>>
{
    public async Task<Result<List<AcademicYearOptionDto>>> Handle(
        GetAllAcademicYearsQuery request,
        CancellationToken cancellationToken
    )
    {
        var repo = unitOfWork.GetOrCreateRepository<AcademicYear, int>();
        var years = await repo.ListAsync(
            new AllAcademicYearsSpecification<AcademicYearOptionDto>(a => new AcademicYearOptionDto
            {
                Id = a.Id,
                Name = a.Title,
            }),
            cancellationToken
        );

        return years;
    }
}
