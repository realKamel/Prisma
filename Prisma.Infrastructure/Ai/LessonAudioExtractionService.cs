using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.Services.StorageService;

namespace Prisma.Infrastructure.Ai;

internal class LessonAudioExtractionService(
    IUnitOfWork uow,
    ITranscriptionService transcription,
    IMuxTokenService muxToken,
    IVideoStorageService videoStorage,
    MuxHttpClient muxHttpClient)
{
    public async Task ExtractAudioAsync(int lessonId, CancellationToken cancellationToken)
    {
        // var repo = uow.GetOrCreateRepository<Lesson, int>();
        // var lesson = await repo.GetByIdAsync(lessonId, cancellationToken);
        // if (lesson is null)
        //     return;
        // var sections = lesson.Sections.Select(x => x.PlaybackId);
        //
        // foreach (var s in sections)
        // {
        //     //var link = videoStorage.GetAudioUrlAsync(s);
        //     var steam = await muxHttpClient.StreamAudioAsync(muxToken.GeneratePlaybackToken(s));
        //     var transcript = await transcription.TranscribeAsync(steam, "", cancellationToken);
        // }
    }
}