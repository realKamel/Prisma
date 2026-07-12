using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.Assistants.Commands.DeleteAssistant;

public record DeleteAssistantCommand(Guid AssistantId) : IRequest<Result>;