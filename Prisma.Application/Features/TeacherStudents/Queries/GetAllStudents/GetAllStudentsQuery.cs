using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetAllStudents;

public record GetAllStudentsQuery : IRequest<List<StudentListItemDto>>;
