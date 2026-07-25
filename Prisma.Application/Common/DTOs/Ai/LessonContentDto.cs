namespace Prisma.Application.Common.DTOs.Ai;

public record LessonContentDto(string LessonTitle, List<string> SectionTitle, string RawTranscript);