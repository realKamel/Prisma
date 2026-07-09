using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

       
        var spec = new ActivityLogsFilterSpec(request.Skip, request.Take + 1);
        var logs = await auditLogRepository.ListAsync(spec, cancellationToken);

        bool hasMore = logs.Count > request.Take;
        var pageLogs = hasMore ? logs.Take(request.Take).ToList() : logs.ToList();

        var eventItems = new List<ActivityEventDto>();

        foreach (var log in pageLogs)
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

            eventItems.Add(new ActivityEventDto(
                CreatedAt: log.CreatedAt ?? DateTimeOffset.UtcNow,
                User: userNameDisplay,
                Role: userRole,
                Action: log.Action ?? string.Empty,
                TableName: log.TableName ?? string.Empty,
                EntityId: log.EntityId ?? string.Empty,
                Detail: ExtractDetail(log.TableName, log.Action, log.OldValues, log.NewValues)
            ));
        }

        ActivityLogStatsDto? statsDto = null;

       
        if (request.Skip == 0)
        {
            var todayDate = DateTimeOffset.UtcNow.Date;

            statsDto = new ActivityLogStatsDto(
                TotalEvents: eventItems.Count,
                TodayEvents: eventItems.Count(e => e.CreatedAt.Date == todayDate),
                ActiveUsers: eventItems.Select(e => e.User).Distinct().Count(),
                Alerts: eventItems.Count(e =>
                    e.Action.Contains("DELETE", StringComparison.OrdinalIgnoreCase) ||
                    e.Action.Contains("REVOKE", StringComparison.OrdinalIgnoreCase))
            );
        }

        return Result<ActivityLogResponseDto>.Success(
            new ActivityLogResponseDto(statsDto, eventItems, hasMore)
        );
    }

    private static readonly Dictionary<string, string[]> DetailFieldCandidates =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["lesson"] = new[] { "Title", "Name" },
            ["academicyear"] = new[] { "Name", "Title" },
            ["assignment"] = new[] { "Title", "Name" },
            ["assignmentsubmission"] = new[] { "FileName", "Title" },
            ["quiz"] = new[] { "Title", "Name" },
            ["question"] = new[] { "Text", "Title" },
            ["section"] = new[] { "Title", "Name" },
            ["redeemcode"] = new[] { "Code" },
            ["report"] = new[] { "Title", "Name" },
            ["lessonmaterial"] = new[] { "Title", "FileName", "Name" },
            ["user"] = new[] { "FirstName", "Email" },
            ["users"] = new[] { "FirstName", "Email" },
        };

    private static string? ExtractDetail(string? tableName, string? action, string? oldValues, string? newValues)
    {
        var t = tableName?.ToLowerInvariant() ?? string.Empty;
        var a = action?.ToLowerInvariant() ?? string.Empty;

        var json = a == "delete" ? (oldValues ?? newValues) : (newValues ?? oldValues);
        if (string.IsNullOrWhiteSpace(json)) return null;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return null;
        }

        using (doc)
        {
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;

            if (t == "payment")
            {
                var amount = TryGetString(root, "Amount");
                var currency = TryGetString(root, "Currency");
                if (amount != null) return currency != null ? $"{amount} {currency}" : amount;
                return null;
            }

            if (!DetailFieldCandidates.TryGetValue(t, out var candidates)) return null;

            foreach (var field in candidates)
            {
                var value = TryGetString(root, field);
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }

            return null;
        }
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        foreach (var prop in root.EnumerateObject())
        {
            if (!string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase)) continue;

            return prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString(),
                JsonValueKind.Number => prop.Value.ToString(),
                _ => null,
            };
        }
        return null;
    }
}