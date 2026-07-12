using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Interfaces;

public interface IExtractionJobQueue
{
    void Enqueue(ExtractionJob job);
    bool TryDequeue(out ExtractionJob? job);
}
