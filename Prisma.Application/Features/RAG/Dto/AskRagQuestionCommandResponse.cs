namespace Prisma.Application.Features.RAG.Dto;

public record AskRagQuestionCommandResponse(Guid SessionId, string Answer);