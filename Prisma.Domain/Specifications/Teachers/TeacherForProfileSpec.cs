using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Teachers;

/// <summary>
/// Loads the teacher tracked, with the profile image file and the storage it lives in
/// (for the bucket name), so profile edits and the image pointer save with the unit of work.
/// </summary>
public sealed class TeacherForProfileSpec : Specification<Teacher>
{
    public TeacherForProfileSpec(Guid teacherId)
    {
        Query
            .Where(t => t.Id == teacherId)
            .Include(t => t.ProfileImageFile)
            .ThenInclude(f => f!.ObjectStorage)
            .AsTracking();
    }
}