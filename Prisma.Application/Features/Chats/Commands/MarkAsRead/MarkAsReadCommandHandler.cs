using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Features.Chats.Commands.MarkAsRead;

internal sealed class MarkAsReadCommandHandler(
    ICurrentUserService currentUserService,
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<MarkAsReadCommand, Result>
{
    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var conversation = await conversationRepository.GetByIdAsync(
            request.ConversationId,
            cancellationToken
        );

        if (conversation is null)
        {
            return Result.NotFound($"Conversation '{request.ConversationId}' was not found.");
        }

        var message = await conversationRepository.GetMessageAsync(
            request.ConversationId,
            request.LastMessageId,
            cancellationToken
        );

        if (message is null)
        {
            return Result.NotFound(
                $"Message '{request.LastMessageId}' was not found in the conversation."
            );
        }

        var readResult = conversation.MarkAsRead(
            userId.Value,
            message.Id,
            message.SentAt,
            DateTimeOffset.UtcNow
        );

        if (!readResult.IsSuccess)
        {
            return Result.Invalid(readResult.ValidationErrors);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
