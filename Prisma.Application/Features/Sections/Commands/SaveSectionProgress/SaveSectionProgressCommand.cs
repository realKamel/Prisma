using Ardalis.Result;
using MediatR;
namespace Prisma.Application.Features.Sections.Commands.SaveSectionProgress;

public record SaveSectionProgressCommand(int SectionId, double WatchedSeconds) : IRequest<Result>;