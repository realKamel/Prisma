using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Quizzes.Queries.GetExtractionStatus;

public class GetExtractionStatusQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetExtractionStatusQueryHandler> logger)
    : IRequestHandler<GetExtractionStatusQuery, Result<ExtractionProgressDto>>
{
    // Must match how ExtractQuestionsFromPdfCommandHandler serializes:
    // PropertyNamingPolicy = CamelCase → keys are camelCase in the DB.
    // PropertyNameCaseInsensitive = true → tolerates any casing on read.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public async Task<Result<ExtractionProgressDto>> Handle(
        GetExtractionStatusQuery request,
        CancellationToken cancellationToken)
    {
        var jobRepo = unitOfWork.GetOrCreateRepository<ExtractionJob, int>();
        var job = await jobRepo.GetByIdAsync(request.JobId, cancellationToken);

        if (job == null)
            return Result<ExtractionProgressDto>.Failure("لم يتم العثور على المهمة");

        // ── Deserialize stored questions ──────────────────────────────────────
        List<ExtractedQuestion> questions;
        try
        {
            questions = JsonSerializer.Deserialize<List<ExtractedQuestion>>(
                job.QuestionsJson, JsonOptions)
                ?? new List<ExtractedQuestion>();

            logger.LogInformation(
                "📋 Job {JobId} | Status: {Status} | QuestionsJson length: {Len} | Parsed: {Count} questions",
                job.Id, job.Status, job.QuestionsJson?.Length ?? 0, questions.Count);

            // Log the raw JSON so you can see exactly what's stored
            if (questions.Count == 0 && !string.IsNullOrEmpty(job.QuestionsJson) && job.QuestionsJson != "[]")
            {
                logger.LogWarning(
                    "⚠️ Job {JobId}: QuestionsJson is non-empty but parsed to 0 questions. Raw JSON (first 500 chars): {Json}",
                    job.Id,
                    job.QuestionsJson[..Math.Min(500, job.QuestionsJson.Length)]);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Job {JobId}: Failed to deserialize QuestionsJson", job.Id);
            questions = new List<ExtractedQuestion>();
        }

        // ── Build progress DTO ────────────────────────────────────────────────
        var progress = job.Status switch
        {
            ExtractionStatus.Pending     => 5,
            ExtractionStatus.Processing  => Math.Min(95, 10 + questions.Count * 5),
            ExtractionStatus.Completed   => 100,
            ExtractionStatus.Failed      => 0,
            _                            => 0
        };

        var phase = job.Status switch
        {
            ExtractionStatus.Pending    => "جاري التحضير...",
            ExtractionStatus.Processing => $"تم استخراج {questions.Count} سؤال...",
            ExtractionStatus.Completed  => "تم الانتهاء!",
            ExtractionStatus.Failed     => job.ErrorMessage ?? "حدث خطأ",
            _                           => "..."
        };

        var currentQuestion = questions.LastOrDefault();

        var dto = new ExtractionProgressDto
        {
            State               = job.Status.ToString().ToLower(),
            Progress            = progress,
            Phase               = phase,
            Error               = job.ErrorMessage,
            CurrentQuestion     = currentQuestion != null ? MapToDto(currentQuestion) : null,
            CompletedQuestions  = questions.Select(MapToDto).ToList()
        };

        return Result<ExtractionProgressDto>.Success(dto, "تم جلب الحالة");
    }

    private static ExtractedQuestionDto MapToDto(ExtractedQuestion q)
    {
        return new ExtractedQuestionDto
        {
            Text   = q.Text,
            Type   = q.Type,
            Degree = q.Score,
            Choices = q.Type == QuestionType.MCQ
                ? q.Options.Select((opt, idx) => new ExtractedChoiceDto
                {
                    Text      = opt,
                    IsCorrect = idx == q.CorrectIndex
                }).ToList()
                : null,
            ModelAnswer = q.Type == QuestionType.Written ? q.ModelAnswer : null,
            IsCorrect   = q.Type == QuestionType.TrueFalse ? q.CorrectBool : null
        };
    }
}