using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Admin.Queries.GetAdminActivitiesQuery;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Users;
using Prisma.Application.Features.Admin.Queries.GetAdminStatsQuery;

namespace Prisma.Application.Features.Users.Queries.GetAdminProfile;

public class GetAdminProfileQueryHandler(
    IUnitOfWork unitOfWork,
    ISender mediator)
    : IRequestHandler<GetAdminProfileQuery, Result<RoleProfileDto>>
{
    public async Task<Result<RoleProfileDto>> Handle(GetAdminProfileQuery request, CancellationToken cancellationToken)
    {
        var userRepo = unitOfWork.GetOrCreateRepository<User, Guid>();
        var user = await userRepo.FirstOrDefaultAsync(new UserByIdSpecification(request.AdminId), cancellationToken);

        if (user is not Domain.Entities.UserAggregate.Admin admin)
            return Result.NotFound($"Admin with id '{request.AdminId}' was not found");

        // NOTE: unlike Teacher/Assistant above, this platform-wide scoping is
        // correct BY DESIGN, not a limitation — there is no per-admin concept
        // anywhere in this system (GetAdminStatsQuery / GetAdminActivitiesQuery
        // are already global). Every admin's profile legitimately shows the
        // same numbers.
        var statsResult = await mediator.Send(new GetAdminStatsQuery(), cancellationToken);
        var activitiesResult = await mediator.Send(new GetAdminActivitiesQuery(), cancellationToken);

        var name = string.Join(" ", new[] { admin.FirstName, admin.SecondName, admin.ThirdName, admin.LastName }
            .Where(p => !string.IsNullOrWhiteSpace(p)));

        var stats = statsResult.Value.Kpis
            .Select(MapKpi)
            .ToList();

        var activities = activitiesResult.Value
            .Select(a =>
                new ProfileActivityDto(a.Details, a.ActivityDate.ToString("yyyy-MM-dd hh:mm tt"), "bg-[var(--purple)]"))
            .ToList();

        return Result<RoleProfileDto>.Success(new RoleProfileDto(name, stats, activities));
    }

    // GetAdminStatsQueryHandler hardcodes these ids in English ("students",
    // "revenue", "lessons-sold", "uptime") — translating + formatting here
    // rather than touching that existing handler.
    private static ProfileStatDto MapKpi(KpiDto k) => k.Id switch
    {
        "students" => new ProfileStatDto("الطلاب", k.Value.ToString("N0"), "text-[var(--purple-lt)]"),
        "revenue" => new ProfileStatDto("الإيرادات", $"{k.Value:N0} ج.م", "text-[var(--mint)]"),
        "lessons-sold" => new ProfileStatDto("الدروس المباعة", k.Value.ToString("N0"), "text-[var(--star)]"),
        "uptime" => new ProfileStatDto("نسبة التشغيل", $"{k.Value:0.#}٪", "text-[var(--coral)]"),
        _ => new ProfileStatDto(k.Id, k.Value.ToString("N0"), "text-[var(--purple-lt)]"),
    };
}