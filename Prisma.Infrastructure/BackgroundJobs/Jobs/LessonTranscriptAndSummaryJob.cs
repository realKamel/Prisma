using MediatR;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Features.Lessons.Commands.LessonTranscriptAndSummary;

namespace Prisma.Infrastructure.BackgroundJobs.Jobs;

public class LessonTranscriptAndSummaryJob(ISender sender) : ILessonTranscriptAndSummarizationJob
{
    public async Task TranscriptAndSummarize(int lessonId, CancellationToken cancellationToken)
    {
        await sender.Send(new LessonTranscriptAndSummarizeCommand(lessonId), cancellationToken);
    }
}