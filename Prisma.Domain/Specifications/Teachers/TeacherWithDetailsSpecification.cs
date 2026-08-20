using Ardalis.Specification;

namespace Prisma.Domain.Specifications.Teachers;

public class TeacherWithDetailsSpecification : Specification<Entities.UserAggregate.Teacher>
{
    public TeacherWithDetailsSpecification()
    {
        Query.Include(t => t.TeacherStudents).Include(t => t.Lessons);
    }

    public TeacherWithDetailsSpecification(Guid teacherId)
    {
        Query
            .AsNoTrackingWithIdentityResolution()
            .Include(t => t.TeacherStudents)
            .Include(t => t.Lessons)
            .Include(t => t.AcademicYears)
            .ThenInclude(ay => ay.AcademicYear)
            .Where(t => t.Id == teacherId)
            .AsSplitQuery();
    }
}
