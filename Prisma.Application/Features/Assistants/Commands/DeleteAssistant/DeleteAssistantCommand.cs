using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Assistants.Commands.DeleteAssistant;

public record DeleteAssistantCommand(Guid AssistantId) : IRequest<Result>;