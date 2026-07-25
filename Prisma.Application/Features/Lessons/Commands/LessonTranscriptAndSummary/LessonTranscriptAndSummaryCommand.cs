using MediatR;

namespace Prisma.Application.Features.Lessons.Commands.LessonTranscriptAndSummary;

public record LessonTranscriptAndSummarizeCommand(int LessonId) : IRequest;
