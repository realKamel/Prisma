using MediatR;
using Microsoft.Extensions.AI;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RAG.Queries.GetSession;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.RAG.Commands.CreateConversation;
//
// public class
//     CreateConversationCommandHandler(IRagQuestionAnswering rag, ICurrentUserService currentUserService, IUnitOfWork uow)
//     : IRequestHandler<CreateConversationCommand,
//         Result<GetDetailedRagSessionQueryResponse>>
// {
//     public async Task<Result<GetDetailedRagSessionQueryResponse>> Handle(CreateConversationCommand request,
//         CancellationToken cancellationToken)
//     {
//         var userId = currentUserService.UserId;
//
//         if (userId is null)
//         {
//             // throw new UnauthorizedException();
//             userId = Guid.Empty;
//         }
//
//         var repo = uow.GetOrCreateRepository<ChatSession, Guid>();
//
//         var answer = await rag.AskAsync(request.Question, null, cancellationToken);
//
//         var title = request.Question.Length > 100 ? request.Question[..100] : request.Question;
//
//         var session = ChatSession.Create(userId, title, answer.ThreadState);
//
//         var chatMessages = rag.GetChatMessagesAsync(answer.session);
//
//         var messages = chatMessages
//             .Where(m => m.Role != ChatRole.System && m.Role != ChatRole.Tool)
//             .Select(m =>
//                 new ChatMessagesDto(m.MessageId, m.Text, m.Role.ToString(),
//                     m.CreatedAt ?? DateTimeOffset.UtcNow))
//             .ToList() ?? [];
//
//         session.Update(answer.ThreadState);
//
//         repo.Add(session);
//
//         await uow.SaveChangesAsync(cancellationToken);
//
//         return new GetDetailedRagSessionQueryResponse(session.Id, messages);
//     }
// }