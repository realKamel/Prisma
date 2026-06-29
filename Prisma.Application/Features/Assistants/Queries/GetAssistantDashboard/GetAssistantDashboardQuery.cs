using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistantDashboard;


public record GetAssistantDashboardQuery : IRequest<Result<GetAssistantDashboardResponse>>;

// ═══════════════════════════════════════════════════════════════
//  Response DTOs
// ═══════════════════════════════════════════════════════════════

public class GetAssistantDashboardResponse
{
    public DashboardTeacher Teacher { get; set; } = new(string.Empty,"أ. احمد مصطفى");
    public List<KpiTile> Kpis { get; set; } = [];
    public List<ActivityItem> Activities { get; set; } = [];
    public List<Permission> Permissions { get; set; } = [];
}

public record DashboardTeacher(string Name, string SupervisorName);

public record KpiTile(
    string Id,
    int Value,
    double Delta,
    string Trend,
    string Variant);

public record ActivityItem(
    int Id,
    string Icon,
    string Message,
    DateTimeOffset CreatedAt
);

public record Permission(
    string Id,
    string Status);