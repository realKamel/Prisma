using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class TeacherByIdSpecification : Specification<Teacher>
{
    public TeacherByIdSpecification(Guid teacherId)
    {
        Query.Where(t => t.Id == teacherId);
    }
}