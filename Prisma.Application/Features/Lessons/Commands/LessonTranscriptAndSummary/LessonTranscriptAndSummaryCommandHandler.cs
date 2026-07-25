using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs.Ai;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Features.Lessons.Commands.LessonTranscriptAndSummary;

internal class LessonTranscriptAndSummaryCommandHandler(
    ITranscriptionService transcriptionService,
    IUnitOfWork uow,
    ISummarizationServices summarizationServices,
    IAudioStreamingService streamingService,
    ITextEmbeddingProcessor textEmbeddingProcessor)
    : IRequestHandler<LessonTranscriptAndSummarizeCommand>
{
    public async Task Handle(LessonTranscriptAndSummarizeCommand request, CancellationToken cancellationToken)
    {
        var lessonRepo = uow.GetOrCreateRepository<Lesson, int>();

        var lesson = await lessonRepo
            .FirstOrDefaultAsync(
                new LessonWithSectionsSpec(request.LessonId), cancellationToken);

        if (lesson is null)
        {
            return;
        }

        var lessonTranscript = new StringBuilder();

        foreach (var s in lesson.Sections)
        {
            if (s.PlaybackId is null)
            {
                continue;
            }

            s.Transcript = await transcriptionService
                .TranscribeAsync(await streamingService
                    .StreamAudioAsync(s.PlaybackId), s.Title, cancellationToken);

            lessonTranscript.AppendLine(s.Transcript);

            // s.TranscriptSummary = await summarizationServices
            //     .SummarizationAsync(s.Transcript, cancellationToken);
        }

        var transcript = lessonTranscript.ToString();
        var summaryDto = new LessonContentDto(
            lesson.Title,
            [..lesson.Sections.Select(x => x.Title)],
            transcript
        );
        lesson.Summary = await summarizationServices
            .SummarizationAsync(summaryDto, cancellationToken);

        lesson.Chunks = await textEmbeddingProcessor.ProcessTextAsync(lesson.Id, transcript, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
    }
}