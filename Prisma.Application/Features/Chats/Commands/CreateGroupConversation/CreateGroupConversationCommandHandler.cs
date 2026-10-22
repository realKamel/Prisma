using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Features.Chats.Commands.CreateGroupConversation;

internal sealed class CreateGroupConversationCommandHandler(
    ICurrentUserService currentUserService,
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateGroupConversationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateGroupConversationCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result<Guid>.Unauthorized();
        }

        var createResult = Conversation.CreateGroup(userId.Value, request.Title, request.MemberIds);

        if (!createResult.IsSuccess)
        {
            return Result<Guid>.Invalid(createResult.ValidationErrors);
        }

        await conversationRepository.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(createResult.Value.Id);
    }
}
