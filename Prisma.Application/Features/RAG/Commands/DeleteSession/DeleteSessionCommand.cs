using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.RAG.Commands.DeleteSession;

public record DeleteSessionCommand(Guid Id) : IRequest<Result>;