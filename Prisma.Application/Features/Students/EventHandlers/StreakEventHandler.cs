using Prisma.Application.Common.Time;
using Prisma.Application.Features.Students.Common;
using Prisma.Domain.DomainEvents.StreakEvents;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Students.EventHandlers;

public static class StreakEventHandler
{
    public static Task Handle(QuizAttemptStartedEvent e, IUnitOfWork uow, CancellationToken ct) =>
        Register(e.StudentId, e.OccurredOn, uow, ct);

    public static Task Handle(AssignmentSubmittedEvent e, IUnitOfWork uow, CancellationToken ct) =>
        Register(e.StudentId, e.OccurredOn, uow, ct);

    public static Task Handle(SectionProgressAdvancedEvent e, IUnitOfWork uow, CancellationToken ct) =>
        Register(e.StudentId, e.OccurredOn, uow, ct);

    private static async Task Register(
        Guid studentId, DateTimeOffset occurredOn, IUnitOfWork uow, CancellationToken ct)
    {
        Console.WriteLine($"Streak event for {studentId} at {occurredOn}");
        var student = await uow.GetOrCreateRepository<Student, Guid>().GetByIdAsync(studentId, ct);

        if (student is not null && student.RegisterActivity(CairoDate.Of(occurredOn)))
            await uow.SaveChangesAsync(ct);
    }
}