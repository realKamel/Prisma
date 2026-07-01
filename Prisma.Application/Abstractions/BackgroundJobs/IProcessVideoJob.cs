namespace Prisma.Application.Abstractions.BackgroundJobs;

public interface IVideoProcessingJob
{
    Task ProcessVideo();
}
