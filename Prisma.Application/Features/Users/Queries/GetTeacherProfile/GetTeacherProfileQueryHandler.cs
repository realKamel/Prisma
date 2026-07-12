using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Users;

namespace Prisma.Application.Features.Users.Queries.GetTeacherProfile;

public class GetTeacherProfileQueryHandler(
    IUnitOfWork unitOfWork,
    ISender mediator)
    : IRequestHandler<GetTeacherProfileQuery, Result<RoleProfileDto>>
{
    public async Task<Result<RoleProfileDto>> Handle(GetTeacherProfileQuery request, CancellationToken cancellationToken)
    {
        var userRepo = unitOfWork.GetOrCreateRepository<User, Guid>();
        var user = await userRepo.FirstOrDefaultAsync(new UserByIdSpecification(request.TeacherId), cancellationToken);

        if (user is not Teacher teacher)
            throw new NotFoundException(nameof(Teacher), request.TeacherId);

        var name = string.Join(" ", new[] { teacher.FirstName, teacher.SecondName, teacher.ThirdName, teacher.LastName }
            .Where(p => !string.IsNullOrWhiteSpace(p)));

        var dashboardResult = await mediator.Send(new GetTeacherDashboardStatusQuery(request.TeacherId), cancellationToken);

        var stats = new List<ProfileStatDto>
        {
            new("أرباح هذا الشهر", $"{dashboardResult.Data.Stats.TotalEarningsForThisMonth:N0}", "text-[var(--purple-lt)]"),
            new("الطلاب النشطون", dashboardResult.Data.Stats.TotalActiveStudents.ToString(), "text-[var(--mint)]"),
            new("الدروس النشطة (عام للمنصة)", dashboardResult.Data.Stats.TotalActiveLessons.ToString(), "text-[var(--star)]"),
            new("دروس مكتملة هذا الشهر", dashboardResult.Data.Stats.TotalCompletedLessonsAgainstThisMonth.ToString(), "text-[var(--coral)]"),
        };

        var activities = dashboardResult.Data.Logs
            .Select(l => new ProfileActivityDto(
                $"{l.Action} — {l.TableName}",
                l.CreatedAt?.ToString("yyyy-MM-dd hh:mm tt") ?? "—",
                "bg-[var(--purple)]"))
            .ToList();

        return Result<RoleProfileDto>.Success(new RoleProfileDto(name, stats, activities));
    }
}