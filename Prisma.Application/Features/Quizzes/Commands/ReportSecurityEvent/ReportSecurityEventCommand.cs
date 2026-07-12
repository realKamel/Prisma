using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Commands.ReportSecurityEvent;

public record ReportSecurityEventCommand(int AttemptId, SecurityEventType EventType) : IRequest<Result>;
