using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Admin.Queries.GetAdminActivitiesQuery;
using Prisma.Application.Features.Admin.Queries.GetAdminStatsQuery;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Admin;

namespace Prisma.Application.Features.Users.Queries.GetAdminProfile;

public class GetAdminProfileQueryHandler(
    IUnitOfWork unitOfWork,
    ISender mediator)
    : IRequestHandler<GetAdminProfileQuery, Result<RoleProfileDto>>
{
    public async Task<Result<RoleProfileDto>> Handle(GetAdminProfileQuery request, CancellationToken cancellationToken)
    {
        var adminRepo = unitOfWork.GetOrCreateRepository<Prisma.Domain.Entities.UserAggregate.Admin, Guid>();

        var adminInfo = await adminRepo.FirstOrDefaultAsync(
            new AdminWithProjectionSpec<AdminNameInfo>(request.AdminId, a =>
                new AdminNameInfo(a.FirstName, a.SecondName, a.ThirdName, a.LastName)),
            cancellationToken);

        if (adminInfo is null)
            return Result.NotFound($"Admin with id '{request.AdminId}' was not found");

        var statsTask = await mediator.Send(new GetAdminStatsQuery(), cancellationToken);
        var activitiesTask = await mediator.Send(new GetAdminActivitiesQuery(), cancellationToken);

        var statsResult = statsTask;
        var activitiesResult = activitiesTask;

        var name = string.Join(" ", new[]
            {
                adminInfo.FirstName, adminInfo.SecondName, adminInfo.ThirdName, adminInfo.LastName
            }
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

    private static ProfileStatDto MapKpi(KpiDto k) => k.Id switch
    {
        "students" => new ProfileStatDto("الطلاب", k.Value.ToString("N0"), "text-[var(--purple-lt)]"),
        "revenue" => new ProfileStatDto("الإيرادات", $"{k.Value:N0} ج.م", "text-[var(--mint)]"),
        "lessons-sold" => new ProfileStatDto("الدروس المباعة", k.Value.ToString("N0"), "text-[var(--star)]"),
        "uptime" => new ProfileStatDto("نسبة التشغيل", $"{k.Value:0.#}٪", "text-[var(--coral)]"),
        _ => new ProfileStatDto(k.Id, k.Value.ToString("N0"), "text-[var(--purple-lt)]"),
    };
    public sealed record AdminNameInfo(string? FirstName, string? SecondName, string? ThirdName, string? LastName);
}