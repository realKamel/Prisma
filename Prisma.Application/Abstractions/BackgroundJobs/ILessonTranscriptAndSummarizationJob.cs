using Hangfire;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Abstractions.BackgroundJobs;

public interface ILessonTranscriptAndSummarizationJob
{
    [Queue(JobQueues.Default)]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    [JobDisplayName("Process Lesson #{0} (Audio, Transcript, Summary)")]
    Task TranscriptAndSummarize(int lessonId, CancellationToken cancellationToken = default);
}