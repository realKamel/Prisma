using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Users.Queries.GetTeacherOptions;

public class GetTeacherOptionsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTeacherOptionsQuery, Result<List<TeacherOptionDto>>>
{
    public async Task<Result<List<TeacherOptionDto>>> Handle(GetTeacherOptionsQuery request, CancellationToken cancellationToken)
    {
        var teacherRepo = unitOfWork.GetOrCreateRepository<Teacher, Guid>();
        var teachers = await teacherRepo.ListAsync(cancellationToken);

        var result = teachers
            .Select(t => new TeacherOptionDto(
                t.Id,
                string.Join(" ", new[] { t.FirstName, t.SecondName, t.ThirdName, t.LastName }
                    .Where(p => !string.IsNullOrWhiteSpace(p)))))
            .ToList();

        return Result<List<TeacherOptionDto>>.Success(result);
    }
}