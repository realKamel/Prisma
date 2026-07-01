using System.Linq.Expressions;
using Hangfire;
using Prisma.Application.Abstractions.BackgroundJobs;

namespace Prisma.Infrastructure.BackgroundJobs;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string Enqueue<T>(Expression<Action<T>> methodCall) =>
         BackgroundJob.Enqueue(methodCall);

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) =>
        BackgroundJob.Enqueue(methodCall);

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay) =>
        BackgroundJob.Schedule(methodCall, delay);

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) =>
        BackgroundJob.Schedule(methodCall, delay);

    public void AddOrUpdateRecurring<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression) =>
        RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);

    public void AddOrUpdateRecurring<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression) =>
        RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);

    public void RemoveRecurring(string jobId) =>
        RecurringJob.RemoveIfExists(jobId);

    public bool Delete(string jobId) =>
        BackgroundJob.Delete(jobId);
}
