using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherStatsQuery;

public record GetTeacherStatsQuery() : IRequest<TeacherStatsDto>;
public record TeacherStatsDto(
    int TotalTeachers,
    int NewTeachersThisMonth,
    int ActiveTeachers,
    decimal MonthRevenue,
    double RevenueChangePercent,
    int TotalStudents
);