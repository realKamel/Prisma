using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentStats;

public record GetStudentStatsQuery(Guid StudentId) : IRequest<StudentStatsDto>;
