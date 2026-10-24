using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Features.Chats.Commands.SendMessage;

internal sealed class SendMessageCommandHandler(
    ICurrentUserService currentUserService,
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SendMessageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result<Guid>.Unauthorized();
        }

        var conversation = await conversationRepository.GetByIdAsync(
            request.ConversationId,
            cancellationToken
        );

        if (conversation is null)
        {
            return Result<Guid>.NotFound($"Conversation '{request.ConversationId}' was not found.");
        }

        var messageResult = conversation.AddMessage(userId.Value, request.Content);

        if (!messageResult.IsSuccess)
        {
            return Result<Guid>.Invalid(messageResult.ValidationErrors);
        }

        // The message is a brand-new entity carrying an application-assigned Guid key, while
        // the conversation's Messages collection is not loaded. Change detection alone would
        // track it as Modified and emit an UPDATE for a row that does not exist yet
        // (DbUpdateConcurrencyException), so register it explicitly for insertion.
        conversationRepository.AddMessage(messageResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(messageResult.Value.Id);
    }
}
