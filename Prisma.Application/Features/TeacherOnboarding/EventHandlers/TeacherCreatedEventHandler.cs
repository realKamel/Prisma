using Prisma.Application.Abstractions.Services.VideoServices;
using Prisma.Domain.DomainEvents;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherOnboarding.EventHandlers;

public static class TeacherCreatedEventHandler
{
    public static async Task Handle(
        TeacherCreatedEvent message,
        IUnitOfWork unitOfWork,
        IVideoLibraryService videoLibraryService,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(videoLibraryService);

        var teacherRepo = unitOfWork.GetOrCreateRepository<Teacher, Guid>();

        var teacher = await teacherRepo.GetByIdAsync(message.TeacherId, cancellationToken);

        ArgumentNullException.ThrowIfNull(teacher);

        var library = await videoLibraryService.CreateLibrary(teacher.FullName, cancellationToken);

        teacher.AttachVideoLibrary(library);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
