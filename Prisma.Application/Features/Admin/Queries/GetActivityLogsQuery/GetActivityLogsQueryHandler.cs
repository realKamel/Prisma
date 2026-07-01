using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;

namespace Prisma.Application.Features.ActivityLogs.Queries.GetActivityLogs;

public class GetActivityLogsQueryHandler(
    IUnitOfWork _unitOfWork,
    UserManager<User> _userManager
) : IRequestHandler<GetActivityLogsQuery, Result<ActivityLogResponseDto>>
{
    public async Task<Result<ActivityLogResponseDto>> Handle(
        GetActivityLogsQuery request,
        CancellationToken cancellationToken)
    {
        var auditLogRepository = _unitOfWork.GetOrCreateRepository<AuditLog, int>();

        var spec = new ActivityLogsFilterSpec(request.Take);
        var logs = await auditLogRepository.ListAsync(spec, cancellationToken);

        var eventItems = new List<ActivityEventDto>();

        foreach (var log in logs)
        {
            string userRole = "system";
            string userNameDisplay = "النظام";

            if (!string.IsNullOrEmpty(log.UserEmail) && !log.UserEmail.StartsWith("system", StringComparison.OrdinalIgnoreCase))
            {
                var user = await _userManager.FindByEmailAsync(log.UserEmail);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var primaryRole = roles.FirstOrDefault();

                    if (!string.IsNullOrEmpty(primaryRole))
                    {
                        userRole = primaryRole.ToLower();
                    }

                    userNameDisplay = $"{user.FirstName} {user.LastName}".Trim();
                    if (string.IsNullOrEmpty(userNameDisplay)) userNameDisplay = user.Email;
                }
            }

            if (!string.Equals(request.Role, "all", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(userRole, request.Role, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            eventItems.Add(new ActivityEventDto(
                CreatedAt: log.CreatedAt ?? DateTimeOffset.UtcNow,
                User: userNameDisplay, 
                Role: userRole,
                Action: log.Action ?? string.Empty,
                TableName: log.TableName ?? string.Empty,
                EntityId: log.EntityId ?? string.Empty
            ));
        }

        int totalEvents = eventItems.Count;

        var todayDate = DateTimeOffset.UtcNow.Date;
        int todayEvents = eventItems.Count(e => e.CreatedAt.Date == todayDate);

        int activeUsersCount = eventItems.Select(e => e.User).Distinct().Count();

        int alertsCount = eventItems.Count(e => e.Action.Contains("DELETE", StringComparison.OrdinalIgnoreCase) || e.Action.Contains("REVOKE", StringComparison.OrdinalIgnoreCase));

        var statsDto = new ActivityLogStatsDto(
            TotalEvents: totalEvents,
            TodayEvents: todayEvents,
            ActiveUsers: activeUsersCount,
            Alerts: alertsCount
        );

        return Result<ActivityLogResponseDto>.Success(
            new ActivityLogResponseDto(statsDto, eventItems)
        );
    }
}