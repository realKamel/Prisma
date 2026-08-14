using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Teachers.Queries.GetTeachers;

public record GetTeachersQuery() : IRequest<Result<List<TeacherDto>>>;

public record TeacherDto(
    string Id,         
    string Name,
    string Phone,
    string Subject,
    int Students,
    decimal Revenue,
    string Status     // active | pending | suspended
);