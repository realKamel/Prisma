using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Features.Chats.Commands.CreateDirectConversation;

internal sealed class CreateDirectConversationCommandHandler(
    ICurrentUserService currentUserService,
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateDirectConversationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateDirectConversationCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result<Guid>.Unauthorized();
        }

        var existing = await conversationRepository.GetDirectConversationAsync(
            userId.Value,
            request.OtherUserId,
            cancellationToken
        );

        if (existing is not null)
        {
            return Result<Guid>.Success(existing.Id);
        }

        var createResult = Conversation.CreateDirect(userId.Value, request.OtherUserId);

        if (!createResult.IsSuccess)
        {
            return Result<Guid>.Invalid(createResult.ValidationErrors);
        }

        await conversationRepository.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(createResult.Value.Id);
    }
}
