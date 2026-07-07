using MediatR;

namespace Prisma.Application.Features.Sections.Commands.CompleteSection;
public record CompleteSectionCommand(int SectionId) : IRequest;