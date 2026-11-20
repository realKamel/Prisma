using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Students.Queries.GetStreakTasks;

public record GetStreakTasksQuery() : IQuery<Result<StreakTasksDto>>;

public sealed record StreakTaskDto(string Type, int? Id, Guid? GuidId, string Title);
public sealed record StreakTasksDto(bool DoneToday, IReadOnlyList<StreakTaskDto> Tasks);