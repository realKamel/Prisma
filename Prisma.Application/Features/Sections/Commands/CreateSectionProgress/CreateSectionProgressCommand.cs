using Ardalis.Result;
using MediatR;
namespace Prisma.Application.Features.Sections.Commands.CreateSectionProgress;

public record CreateSectionProgressCommand(int SectionId) : IRequest<Result>;