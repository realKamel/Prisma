using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
namespace Prisma.Domain.Specifications.Lessons;

public class TeacherLessonsWithProjectionSpec<TResult> : Specification<Lesson, TResult>
{
    public TeacherLessonsWithProjectionSpec(
        Guid teacherId,
        int pageNumber,
        int pageSize,
        string? search,
        LessonStatus? status,
        TeacherLessonSort sort,
        Expression<Func<Lesson, TResult>> projection
    )
    {
        var term = search?.Trim().ToLower();
        var hasSearch = !string.IsNullOrEmpty(term);

        Query.Where(l => l.TeacherId == teacherId).AsNoTrackingWithIdentityResolution();
        Query.Where(l => l.Status == status, status.HasValue);
        Query.Where(l => l.Title != null && l.Title.ToLower().Contains(term!), hasSearch);

        // Id is the tie-breaker so paging stays stable when sort values are equal.
        switch (sort)
        {
            case TeacherLessonSort.MostStudents:
                Query.OrderByDescending(l => l.Enrollments.Count).ThenByDescending(l => l.Id);
                break;
            case TeacherLessonSort.HighestRevenue:
                // Sums raw amounts: lessons in different currencies are compared as plain numbers.
                Query
                    .OrderByDescending(l =>
                        l.Payments.Where(p => p.Status == PaymentStatus.Completed)
                            .Sum(p => p.Money.Amount)
                    )
                    .ThenByDescending(l => l.Id);
                break;
            case TeacherLessonSort.PriceHigh:
                Query.OrderByDescending(l => l.Money.Amount).ThenByDescending(l => l.Id);
                break;
            case TeacherLessonSort.PriceLow:
                Query.OrderBy(l => l.Money.Amount).ThenByDescending(l => l.Id);
                break;
            default:
                Query.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id);
                break;
        }

        var size = Math.Clamp(pageSize, 1, 50);
        var page = Math.Max(pageNumber, 1);

        Query.Skip((page - 1) * size).Take(size).Select(projection);
    }
}

