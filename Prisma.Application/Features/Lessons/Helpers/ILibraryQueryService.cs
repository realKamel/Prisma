using Prisma.Application.Features.Lessons.Dtos;

namespace Prisma.Application.Features.Lessons.Helpers;

public interface ILibraryQueryService
{
    Task<LibraryMainInfoReadModel?> GetLibraryInfoFromTeacher(
        Guid? teacherId,
        CancellationToken cancellationToken
    );

    Task<LibraryMainInfoReadModel?> GetLibraryInfoFromAssistant(
        Guid assistantId,
        CancellationToken cancellationToken
    );
}
