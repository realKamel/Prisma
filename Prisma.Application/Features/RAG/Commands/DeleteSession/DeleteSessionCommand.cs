using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.RAG.Commands.DeleteSession;

public record DeleteSessionCommand(Guid Id) : IRequest<Result>;