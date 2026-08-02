using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentDetails;

public record GetStudentDetailsQuery(Guid StudentId) : IRequest<Result<StudentListItemDto>>;
