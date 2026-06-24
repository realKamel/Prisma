using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Services;

public class OpenAiExamExtractor : IOpenAiExamExtractor
{
    private readonly ChatClient? _chatClient;
    private readonly ILogger<OpenAiExamExtractor> _logger;
    private readonly bool _demoMode;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public OpenAiExamExtractor(IConfiguration configuration, ILogger<OpenAiExamExtractor> logger)
    {
        _logger   = logger;
        _demoMode = configuration.GetValue<bool>("OpenAI:DemoMode");

        _logger.LogInformation("🔧 OpenAiExamExtractor init — DemoMode={Demo}", _demoMode);

        if (_demoMode)
        {
            _logger.LogInformation("🎮 Running in DEMO MODE");
            return;
        }

        var apiKey = configuration["OpenAI:ApiKey"];
        _logger.LogInformation("🔑 OpenAI ApiKey present={Present}", !string.IsNullOrEmpty(apiKey));

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("⚠️ No API key — falling back to DEMO MODE");
            _demoMode = true;
            return;
        }

        _chatClient = new ChatClient(model: "gpt-4o", apiKey: apiKey);
        _logger.LogInformation("✅ ChatClient created");
    }

    public async IAsyncEnumerable<ExtractionProgress> ExtractQuestionsAsync(
        string pdfText,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "🚀 ExtractQuestionsAsync — pdfText length={Len}, demoMode={Demo}",
            pdfText?.Length ?? 0, _demoMode);

        yield return new ExtractionProgress
        {
            ProgressPercent    = 10,
            Phase              = "جاري قراءة الملف...",
            CompletedQuestions = new List<ExtractedQuestion>()
        };

        await Task.Delay(200, cancellationToken);

        yield return new ExtractionProgress
        {
            ProgressPercent    = 30,
            Phase              = "جاري تحليل محتوى PDF...",
            CompletedQuestions = new List<ExtractedQuestion>()
        };

        await Task.Delay(200, cancellationToken);

        // ── Get questions outside try/catch so we can yield inside the iterator
        // C# does not allow `yield` inside a catch block, so we capture the
        // result (or error) in local variables and yield after the try/catch.
        List<ExtractedQuestion> questions = new();
        string? extractionError = null;

        try
        {
            questions = await GetQuestionsAsync(pdfText, cancellationToken);
            _logger.LogInformation("📊 GetQuestionsAsync returned {Count} questions", questions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GetQuestionsAsync threw");
            extractionError = ex.Message;
        }

        // ── Yield error or empty result outside the catch ─────────────────────
        if (extractionError is not null)
        {
            yield return new ExtractionProgress
            {
                ProgressPercent    = 0,
                Phase              = $"فشل الاستخراج: {extractionError}",
                IsComplete         = true,
                CompletedQuestions = new List<ExtractedQuestion>()
            };
            yield break;
        }

        if (questions.Count == 0)
        {
            _logger.LogWarning("⚠️ No questions extracted");
            yield return new ExtractionProgress
            {
                ProgressPercent    = 100,
                Phase              = "لم يتم العثور على أسئلة في الملف",
                IsComplete         = true,
                CompletedQuestions = new List<ExtractedQuestion>()
            };
            yield break;
        }

        // ── Stream each question ──────────────────────────────────────────────
        var completed = new List<ExtractedQuestion>();
        for (int i = 0; i < questions.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            completed.Add(questions[i]);

            _logger.LogInformation(
                "📤 Question {Idx}/{Total}: [{Type}] {Text}",
                i + 1, questions.Count, questions[i].Type,
                (questions[i].Text ?? "")[..Math.Min(40, questions[i].Text?.Length ?? 0)]);

            yield return new ExtractionProgress
            {
                ProgressPercent    = 50 + (i + 1) * 50 / questions.Count,
                Phase              = $"تم استخراج {completed.Count} من {questions.Count} سؤال...",
                CurrentQuestion    = questions[i],
                CompletedQuestions = new List<ExtractedQuestion>(completed)
            };

            await Task.Delay(100, cancellationToken);
        }

        _logger.LogInformation("✅ Final yield — {Count} questions", completed.Count);

        yield return new ExtractionProgress
        {
            ProgressPercent    = 100,
            Phase              = "تم الانتهاء!",
            IsComplete         = true,
            CurrentQuestion    = null,
            CompletedQuestions = completed
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task<List<ExtractedQuestion>> GetQuestionsAsync(
        string pdfText, CancellationToken cancellationToken)
    {
        if (_demoMode)
        {
            _logger.LogInformation("🎮 Returning demo questions");
            await Task.Delay(300, cancellationToken);
            return GetDemoQuestions();
        }

        if (_chatClient == null)
        {
            _logger.LogError("❌ ChatClient is null");
            return new List<ExtractedQuestion>();
        }

        _logger.LogInformation("🤖 Calling OpenAI — pdfText length={Len}", pdfText?.Length ?? 0);

        try
        {
            var systemPrompt = """
                You are an exam question extractor. Given text from a PDF, extract ALL questions.
                Return ONLY valid JSON — no markdown, no explanation, no code fences.
                Format:
                {
                  "questions": [
                    {
                      "text": "question text here",
                      "type": "MCQ",
                      "options": ["option A", "option B", "option C", "option D"],
                      "correctIndex": 0,
                      "correctBool": null,
                      "modelAnswer": "",
                      "score": 2
                    }
                  ]
                }
                Rules:
                - type must be exactly "MCQ", "TrueFalse", or "Written"
                - For MCQ: fill options, set correctIndex (0-based), correctBool=null
                - For TrueFalse: options=[], correctIndex=null, correctBool=true or false
                - For Written: options=[], correctIndex=null, correctBool=null, fill modelAnswer
                - score is point value (default 2 if not specified)
                - Extract every question, do not skip any
                """;

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(
                    string.IsNullOrWhiteSpace(pdfText)
                        ? "No text could be extracted. Return {\"questions\":[]}"
                        : $"Extract all questions from this text:\n\n{pdfText}")
            };

            var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            var response   = completion.Value.Content[0].Text;

            _logger.LogInformation("🤖 OpenAI response length={Len}", response?.Length ?? 0);
            _logger.LogInformation("🤖 OpenAI raw (first 300): {Raw}",
                (response ?? "")[..Math.Min(300, response?.Length ?? 0)]);

            var parsed = ParseJson(response ?? "{}");
            _logger.LogInformation("🔍 Parsed {Count} questions", parsed.Count);
            return parsed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ OpenAI call failed");
            return new List<ExtractedQuestion>();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    private List<ExtractedQuestion> ParseJson(string response)
    {
        try
        {
            var json = ExtractJson(response);
            _logger.LogInformation("🔍 JSON to parse (first 200): {Json}",
                json[..Math.Min(200, json.Length)]);

            var result    = JsonSerializer.Deserialize<ExtractionResult>(json, JsonOptions);
            var questions = result?.Questions ?? new List<QuestionData>();

            _logger.LogInformation("🔍 Deserialized {Count} items", questions.Count);

            return questions.Select(q => new ExtractedQuestion
            {
                Text         = q.Text ?? "",
                Type         = ParseType(q.Type),
                Options      = q.Options ?? new List<string>(),
                CorrectIndex = q.CorrectIndex,
                CorrectBool  = q.CorrectBool,
                ModelAnswer  = q.ModelAnswer ?? "",
                Score        = q.Score > 0 ? q.Score : 10
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ JSON parse failed. Raw: {Raw}", response);
            return new List<ExtractedQuestion>();
        }
    }

    private static string ExtractJson(string response)
    {
        var trimmed = response.Trim();

        // Strip markdown code fences if present
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            var lastFence    = trimmed.LastIndexOf("```");
            if (firstNewline >= 0 && lastFence > firstNewline)
                trimmed = trimmed[(firstNewline + 1)..lastFence].Trim();
        }

        var start = trimmed.IndexOf('{');
        var end   = trimmed.LastIndexOf('}');
        return start >= 0 && end > start ? trimmed[start..(end + 1)] : trimmed;
    }

    private static QuestionType ParseType(string? type) =>
        type?.Trim().ToUpperInvariant() switch
        {
            "MCQ"                                => QuestionType.MCQ,
            "TRUEFALSE" or "TF" or "TRUE_FALSE"  => QuestionType.TrueFalse,
            "WRITTEN"                            => QuestionType.Written,
            _                                    => QuestionType.MCQ
        };

    // ─────────────────────────────────────────────────────────────────────────
    private List<ExtractedQuestion> GetDemoQuestions() =>
    [
        new()
        {
            Text = "أي من التالي يعتبر من الغازات النبيلة؟",
            Type = QuestionType.MCQ,
            Options = ["النيتروجين", "الأرجون", "الأكسجين", "ثاني أكسيد الكربون"],
            CorrectIndex = 1, CorrectBool = null, ModelAnswer = string.Empty, Score = 2
        },
        new()
        {
            Text = "الجاذبية الأرضية تزداد كلما اقتربنا من مركز الأرض.",
            Type = QuestionType.TrueFalse,
            Options = [], CorrectIndex = null, CorrectBool = true, ModelAnswer = string.Empty, Score = 1
        },
        new()
        {
            Text = "ما هو pH المحايد؟",
            Type = QuestionType.MCQ,
            Options = ["0", "7", "14", "1"],
            CorrectIndex = 1, CorrectBool = null, ModelAnswer = string.Empty, Score = 2
        },
        new()
        {
            Text = "اشرح بالتفصيل كيفية تكون البراكين.",
            Type = QuestionType.Written,
            Options = [], CorrectIndex = null, CorrectBool = null,
            ModelAnswer = "تتكون البراكين نتيجة لحركة الصفائح التكتونية...", Score = 5
        },
        new()
        {
            Text = "الضوء يتبع قانون الانعكاس.",
            Type = QuestionType.TrueFalse,
            Options = [], CorrectIndex = null, CorrectBool = true, ModelAnswer = string.Empty, Score = 1
        },
        new()
        {
            Text = "وحدة قياس القوة في النظام الدولي هي:",
            Type = QuestionType.MCQ,
            Options = ["واط", "نيوتن", "جول", "باسكال"],
            CorrectIndex = 1, CorrectBool = null, ModelAnswer = string.Empty, Score = 2
        },
        new()
        {
            Text = "ما هي أهمية طبقة الأوزون؟",
            Type = QuestionType.Written,
            Options = [], CorrectIndex = null, CorrectBool = null,
            ModelAnswer = "تحمي من الأشعة فوق البنفسجية...", Score = 5
        }
    ];

    // ─────────────────────────────────────────────────────────────────────────
    private class ExtractionResult
    {
        [JsonPropertyName("questions")]
        public List<QuestionData> Questions { get; set; } = [];
    }

    private class QuestionData
    {
        [JsonPropertyName("text")]         public string?       Text         { get; set; }
        [JsonPropertyName("type")]         public string        Type         { get; set; } = "";
        [JsonPropertyName("options")]      public List<string>? Options      { get; set; }
        [JsonPropertyName("correctIndex")] public int?          CorrectIndex { get; set; }
        [JsonPropertyName("correctBool")]  public bool?         CorrectBool  { get; set; }
        [JsonPropertyName("modelAnswer")]  public string?       ModelAnswer  { get; set; }
        [JsonPropertyName("score")]        public int           Score        { get; set; }
    }
}