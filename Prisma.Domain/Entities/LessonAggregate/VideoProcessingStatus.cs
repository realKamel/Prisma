namespace Prisma.Domain.Entities.LessonAggregate;

public enum VideoProcessingStatus
{
    Created = 0,
    Uploaded = 1,
    Processing = 2,
    Transcoding = 3,
    Finished = 4,
    Error = 5,
}
