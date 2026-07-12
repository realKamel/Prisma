using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentActivities;

public record GetStudentActivitiesQuery(Guid StudentId) : IRequest<List<StudentActivityDto>>;
