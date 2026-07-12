using System.Threading.Channels;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Services;

public class ExtractionJobQueue : IExtractionJobQueue
{
    private readonly Channel<ExtractionJob> _channel = Channel.CreateUnbounded<ExtractionJob>();

    public void Enqueue(ExtractionJob job)
    {
        _channel.Writer.TryWrite(job);
    }

    public bool TryDequeue(out ExtractionJob? job)
    {
        return _channel.Reader.TryRead(out job);
    }
}
