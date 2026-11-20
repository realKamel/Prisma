using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.StreakEvents;

public record QuizAttemptStartedEvent(Guid StudentId) : BaseDomainEvent;
public record AssignmentSubmittedEvent(Guid StudentId) : BaseDomainEvent;
public record SectionProgressAdvancedEvent(Guid StudentId, int SectionId) : BaseDomainEvent;