using MediatR;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllUsersQuery, Result<List<UserListItemDto>>>
{
    public async Task<Result<List<UserListItemDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var studentRepo   = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var teacherRepo   = unitOfWork.GetOrCreateRepository<Teacher, Guid>();
        var assistantRepo = unitOfWork.GetOrCreateRepository<Assistant, Guid>();
        var adminRepo     = unitOfWork.GetOrCreateRepository<Admin, Guid>();

        var students   = (await studentRepo.ListAsync(cancellationToken)).Where(u => !u.IsDeleted);
        var teachers   = (await teacherRepo.ListAsync(cancellationToken)).Where(u => !u.IsDeleted);
        var assistants = (await assistantRepo.ListAsync(cancellationToken)).Where(u => !u.IsDeleted);
        var admins     = (await adminRepo.ListAsync(cancellationToken)).Where(u => !u.IsDeleted);

        var result = new List<UserListItemDto>();
        result.AddRange(students.Select(u => Map(u, AppRoles.Student)));
        result.AddRange(teachers.Select(u => Map(u, AppRoles.Teacher)));
        result.AddRange(assistants.Select(u => Map(u, AppRoles.Assistant)));
        result.AddRange(admins.Select(u => Map(u, AppRoles.Admin)));

        return Result<List<UserListItemDto>>.Success(
            result.OrderByDescending(u => u.Joined).ToList());
    }

    private static UserListItemDto Map(User user, string role)
    {
        var name = string.Join(" ", new[] { user.FirstName, user.SecondName, user.ThirdName, user.LastName }
            .Where(p => !string.IsNullOrWhiteSpace(p)));

        return new UserListItemDto(
            user.Id,
            name,
            user.Email ?? string.Empty,
            role,
            !user.IsBlocked,
            (user.CreatedAt ?? DateTimeOffset.UtcNow).ToString("yyyy-MM-dd"),
            HumanizeLastActive(user.UpdatedAt ?? user.CreatedAt));
    }

    // NOTE: there's no LastLoginAt/LastActiveAt column, so this approximates
    // "activity" from UpdatedAt. It's a stand-in, not a real activity signal.
    private static string HumanizeLastActive(DateTimeOffset? dt)
    {
        if (dt is null) return "—";
        var diff = DateTimeOffset.UtcNow - dt.Value;
        if (diff.TotalMinutes < 1) return "الآن";
        if (diff.TotalHours   < 1) return $"منذ {(int)diff.TotalMinutes} دقيقة";
        if (diff.TotalDays    < 1) return $"منذ {(int)diff.TotalHours} ساعة";
        if (diff.TotalDays    < 2) return "منذ يوم";
        if (diff.TotalDays    < 7) return $"منذ {(int)diff.TotalDays} أيام";
        if (diff.TotalDays    < 30) return "منذ أسبوع";
        return "منذ فترة";
    }
}