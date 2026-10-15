using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.AcademicYears.Dtos;

namespace Prisma.Application.Features.AcademicYearTeacher.Queries.GetAllAcademicYearTeacher;

public record GetAllAcademicYearTeacherQuery : IRequest<Result<List<AcademicYearOptionDto>>>;

