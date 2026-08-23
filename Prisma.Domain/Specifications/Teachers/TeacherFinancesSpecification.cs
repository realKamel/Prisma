using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Teachers;

public class TeacherFinancesSpecification<TResult> : Specification<Payment, TResult>
{
    public TeacherFinancesSpecification(Guid teacherId, Expression<Func<Payment, TResult>> projection)
    {
        Query.Where(p => p.Lesson != null && p.Lesson.TeacherId == teacherId)
            .Where(p => p.Status == PaymentStatus.Completed)
            .Include(p => p.Student)
            .Include(p => p.Lesson)
            .OrderByDescending(p => p.PaidAt)
            .AsNoTracking()
            .Select(projection);
    }
}
