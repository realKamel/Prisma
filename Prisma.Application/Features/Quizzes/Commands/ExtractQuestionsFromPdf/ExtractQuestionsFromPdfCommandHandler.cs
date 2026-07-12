using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Quizzes.Commands.ExtractQuestionsFromPdf;

public class ExtractQuestionsFromPdfCommandHandler(
    IUnitOfWork unitOfWork,
    IPdfTextExtractor pdfExtractor,
    IOpenAiExamExtractor aiExtractor,
    ILogger<ExtractQuestionsFromPdfCommandHandler> logger)
    : IRequestHandler<ExtractQuestionsFromPdfCommand, Result<ExtractionJobDto>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<Result<ExtractionJobDto>> Handle(
        ExtractQuestionsFromPdfCommand request,
        CancellationToken cancellationToken)
    {
        var jobRepo = unitOfWork.GetOrCreateRepository<ExtractionJob, int>();

        var job = new ExtractionJob
        {
            FileName = request.FileName,
            FilePath = request.FilePath,
            Status = ExtractionStatus.Processing,
            QuestionsJson = "[]"
        };

        jobRepo.Add(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var jobId = job.Id;

        logger.LogInformation("🆕 Job {JobId} created for file: {FileName}", jobId, request.FileName);

        try
        {
            // ── 1. Extract PDF text ───────────────────────────────────────────
            var pdfText = await pdfExtractor.ExtractTextAsync(job.FilePath, cancellationToken);
            logger.LogInformation("📄 Job {JobId}: PDF text extracted, length = {Len} chars", jobId, pdfText?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(pdfText))
                logger.LogWarning("⚠️ Job {JobId}: PDF text is empty — extraction will produce 0 questions", jobId);

            // ── 2. Run AI extraction ──────────────────────────────────────────
            // Always keep the largest snapshot seen; break early on IsComplete.
            List<ExtractedQuestion> completed = new();
            int progressCount = 0;

            await foreach (var progress in aiExtractor.ExtractQuestionsAsync(pdfText, cancellationToken))
            {
                progressCount++;
                logger.LogInformation(
                    "🔄 Job {JobId}: progress #{N} — IsComplete={Done}, questions in update={Count}",
                    jobId, progressCount, progress.IsComplete, progress.CompletedQuestions.Count);

                if (progress.CompletedQuestions.Count > completed.Count)
                    completed = progress.CompletedQuestions;

                if (progress.IsComplete)
                    break;
            }

            logger.LogInformation(
                "✅ Job {JobId}: done — {Count} questions after {N} updates",
                jobId, completed.Count, progressCount);

            // ── 3. Persist results ────────────────────────────────────────────
            var trackedJob = await jobRepo.GetByIdAsync(jobId, cancellationToken)
                             ?? throw new InvalidOperationException($"ExtractionJob {jobId} not found.");

            trackedJob.QuestionsJson = JsonSerializer.Serialize(completed, JsonOptions);
            trackedJob.Status        = ExtractionStatus.Completed;
            trackedJob.CompletedAt   = DateTime.UtcNow;

            logger.LogInformation(
                "💾 Job {JobId}: saving QuestionsJson (length={Len})",
                jobId, trackedJob.QuestionsJson.Length);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<ExtractionJobDto>.Success(new ExtractionJobDto
            {
                JobId    = jobId,
                FileName = request.FileName,
                Status   = "completed"
            }, $"تم استخراج {completed.Count} سؤال بنجاح");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Job {JobId}: extraction failed", jobId);

            try
            {
                var failedJob = await jobRepo.GetByIdAsync(jobId, cancellationToken);
                if (failedJob is not null)
                {
                    failedJob.Status       = ExtractionStatus.Failed;
                    failedJob.ErrorMessage = ex.Message;
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            catch { /* swallow */ }

            return Result<ExtractionJobDto>.Failure($"خطأ: {ex.Message}");
        }
    }
}