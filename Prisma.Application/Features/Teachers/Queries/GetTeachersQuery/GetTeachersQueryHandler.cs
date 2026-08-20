using Ardalis.Result;
using MediatR;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;
using TeacherEntity = Prisma.Domain.Entities.UserAggregate.Teacher;

namespace Prisma.Application.Features.Teachers.Queries.GetTeachersQuery;

public class GetTeachersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTeachersQuery, Result<List<TeacherDto>>>
{
    public async Task<Result<List<TeacherDto>>> Handle(
        GetTeachersQuery request,
        CancellationToken cancellationToken
    )
    {
        var teachers = await unitOfWork
            .GetOrCreateRepository<TeacherEntity, Guid>()
            .ListAsync(new TeacherWithDetailsSpecification(), cancellationToken);

        var payments = await unitOfWork
            .GetOrCreateRepository<Payment, int>()
            .ListAsync(new TeacherFinancesSpec(), cancellationToken);

        var teacherDtos = new List<TeacherDto>();

        foreach (var teacher in teachers)
        {
            string name = $"{teacher.FirstName} {teacher.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = teacher.UserName ?? "معلم بالمنصة";
            }

            var teacherLessonIds = teacher.Lessons?.Select(l => l.Id).ToList() ?? [];

            decimal revenue = payments
                .Where(p => teacherLessonIds.Contains(p.LessonId))
                .Sum(p => p.Amount);

            string status = teacher.Status == TeacherStatus.Suspended ? "suspended" : "active";

            var dto = new TeacherDto(
                Id: teacher.Id.ToString(),
                Name: name,
                Phone: teacher.PhoneNumber ?? string.Empty,
                Subject: string.IsNullOrWhiteSpace(teacher.Subject) ? "عام" : teacher.Subject,
                Students: teacher.TeacherStudents?.Count ?? 0,
                Revenue: revenue,
                Status: status
            );

            teacherDtos.Add(dto);
        }

        return Result<List<TeacherDto>>.Success(teacherDtos);
    }
}
