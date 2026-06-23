using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.AcademicYears.Dtos;

namespace Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;

public record GetAllAcademicYearsQuery : IRequest<Result<List<AcademicYearOptionDto>>>;
