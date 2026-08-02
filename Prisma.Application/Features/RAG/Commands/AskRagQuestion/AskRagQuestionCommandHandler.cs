using System.Runtime.CompilerServices;
using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.RAG.Dto;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.RAG.Commands.AskRagQuestion;

public class AskRagQuestionCommandHandler(
    IRagQuestionAnswering ragService,
    ICurrentUserService currentUserService,
    IUnitOfWork uow) : IStreamRequestHandler<AskRagQuestionCommand, Result<AskRagQuestionCommandResponse>>
{
    public async IAsyncEnumerable<Result<AskRagQuestionCommandResponse>> Handle(
        AskRagQuestionCommand request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var sessionRepo = uow.GetOrCreateRepository<ChatSession, Guid>();

        // 1. Get or Create the Session FIRST so we have a guaranteed ID for the stream
        var session = request.SessionId is { } id
            ? await sessionRepo.GetByIdAsync(id, cancellationToken)
            : null;

        if (session is null)
        {
            var title = request.Question.Length > 100 ? request.Question[..100] : request.Question;
            // Initialize with an empty state; we will update it at the end of the stream
            session = ChatSession.Create(currentUserService.UserId, title, "{}");
            sessionRepo.Add(session);

            // Save immediately so the session gets its tracking ID generated
            await uow.SaveChangesAsync(cancellationToken);
        }

        // 2. Fetch the stream from your RAG service
        var answerStream = ragService.AskAsync(
            request.Question,
            session.SerializedSessionJson,
            cancellationToken);

        var answerText = new StringBuilder();
        var finalThreadState = session.SerializedSessionJson;

        // 3. Stream the tokens immediately to Angular
        await foreach (var item in answerStream.ConfigureAwait(false))
        {
            answerText.Append(item.Text);

            // Capture the thread state as it arrives in the stream chunks
            if (!string.IsNullOrEmpty(item.ThreadState))
            {
                finalThreadState = item.ThreadState;
            }

            yield return new AskRagQuestionCommandResponse(session.Id, item.Text);
        }

        // 4. Update the session metadata now that the stream has completely finished
        session.Update(finalThreadState);
        sessionRepo.Update(session);

        await uow.SaveChangesAsync(cancellationToken);
    }
}