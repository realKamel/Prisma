using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.RAG.Dto;

namespace Prisma.Application.Features.RAG.Commands.AskRagQuestion;

public record AskRagQuestionCommand(Guid? SessionId, string Question)
    : IStreamRequest<Result<AskRagQuestionCommandResponse>>;