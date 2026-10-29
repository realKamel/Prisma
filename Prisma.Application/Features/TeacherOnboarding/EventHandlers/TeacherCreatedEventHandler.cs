using Prisma.Application.Abstractions.Services;
using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Application.Abstractions.Services.VideoServices;
using Prisma.Domain.DomainEvents;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherOnboarding.EventHandlers;

public static class TeacherCreatedEventHandler
{
    public static async Task Handle(
        TeacherCreatedEvent @event,
        IUnitOfWork unitOfWork,
        IVideoLibraryService videoLibraryService,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(videoLibraryService);

        var teacherRepo = unitOfWork.GetOrCreateRepository<Teacher, Guid>();

        var teacher = await teacherRepo.GetByIdAsync(@event.TeacherId, cancellationToken);

        ArgumentNullException.ThrowIfNull(teacher);

        var library = await videoLibraryService.CreateLibrary(teacher.FullName, cancellationToken);

        teacher.AttachVideoLibrary(library);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
