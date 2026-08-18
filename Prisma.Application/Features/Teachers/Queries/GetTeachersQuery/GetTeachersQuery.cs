using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Teachers.Queries.GetTeachersQuery;

public record GetTeachersQuery() : IRequest<Result<List<TeacherDto>>>;

public record TeacherDto(
    string Id,
    string Name,
    string Phone,
    string Subject,
    int Students,
    decimal Revenue,
    string Status // active | pending | suspended
);