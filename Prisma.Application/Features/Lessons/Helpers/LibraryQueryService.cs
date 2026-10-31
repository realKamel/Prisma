using Prisma.Application.Features.Lessons.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Domain.Specifications.VideoLibrarySpecs;

namespace Prisma.Application.Features.Lessons.Helpers;

internal sealed class LibraryQueryService(IUnitOfWork unitOfWork) : ILibraryQueryService
{
    public async Task<LibraryMainInfoReadModel?> GetLibraryInfoFromTeacher(
        Guid? teacherId,
        CancellationToken cancellationToken
    )
    {
        var repository = unitOfWork.GetOrCreateRepository<VideoLibrary, int>();

        if (!teacherId.HasValue)
        {
            return null;
        }

        var spec = new VideoLibraryByTeacherIdWithProjection<LibraryMainInfoReadModel>(
            teacherId: teacherId.Value,
            library => new LibraryMainInfoReadModel(
                library.Id,
                library.ProviderLibraryId,
                library.LibraryApiKey
            )
        );

        return await repository.FirstOrDefaultAsync(spec, cancellationToken);
    }

    public async Task<LibraryMainInfoReadModel?> GetLibraryInfoFromAssistant(
        Guid assistantId,
        CancellationToken cancellationToken
    )
    {
        var assistantRepository = unitOfWork.GetOrCreateRepository<Assistant, Guid>();
        var teacherId = await assistantRepository.FirstOrDefaultAsync(
            new AssistantByIdWithProjectionSpec<Guid?>(userId: assistantId, (x) => x.TeacherId),
            cancellationToken
        );

        return await GetLibraryInfoFromTeacher(teacherId: teacherId, cancellationToken);
    }
}
