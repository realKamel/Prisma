using System.Linq.Expressions;

namespace Prisma.Application.Abstractions.BackgroundJobs;

public interface IBackgroundJobService
{
    string Enqueue<T>(Expression<Action<T>> methodCall);
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
    string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    void AddOrUpdateRecurring<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression);
    void AddOrUpdateRecurring<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression);
    void RemoveRecurring(string jobId);
    bool Delete(string jobId);
}